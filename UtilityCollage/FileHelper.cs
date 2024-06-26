﻿using NLog;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace UtilityCollage
{

    /// <summary>
    /// Clase con utilitarios para manejo de directorios y ficheros.
    /// </summary>
    public partial class FileHelper
    {

        private static readonly Logger _log = NLog.LogManager.GetCurrentClassLogger();

        [GeneratedRegex("[A-Za-z]+(:\\\\){1}.*")]
        private static partial Regex RutaRelativaRE();

        public static bool EsRutaRelativa(string ruta)
        {
            Regex reRuta = RutaRelativaRE();
            return !reRuta.Match(ruta).Success;
        }

        /// <summary>
        /// Elimina de forma recursiva un directorio.
        /// </summary>
        /// <param name="ruta"></param>
        public static void EliminarDirectorio(string ruta)
        {
            if (!Directory.Exists(ruta))
            {
                return;
            }
            _log.Trace($"[DIR] {ruta}");
            DirectoryInfo di = new DirectoryInfo(ruta);
            foreach (FileInfo fi in di.GetFiles())
            {
                _log.Trace($"  [FILE] {fi.Name}");
                if (fi.IsReadOnly)
                {
                    fi.Attributes &= ~FileAttributes.ReadOnly;
                }
                fi.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                _log.Trace($"  [SUBDIR]--->");
                EliminarDirectorio(dir.FullName);
            }
            di.Delete();
            _log.Trace($"[DELETE] {ruta}");
        }

        /// <summary>
        /// Crea de forma recursiva un directorio.
        /// </summary>
        /// <param name="ruta"></param>
        public static void CrearDirectorio(string ruta)
        {
            if (ruta == null || ruta == "" || !new Regex("^[A-Za-z]+(:){1}(\\\\){1,2}.*$").Match(ruta).Success)
            {
                return;
            }
            string unidad = ruta.Substring(0, ruta.IndexOf(':') + 1);
            string folders = ruta.Substring(ruta.IndexOf(':') + 1);
            string[] carpetas = folders.Split('\\');
            folders = $"{unidad}\\";
            foreach (string carpeta in carpetas)
            {
                if (carpeta == "")
                {
                    continue;
                }
                folders = $"{folders}{carpeta}\\";
                if (!Directory.Exists(folders))
                {
                    Directory.CreateDirectory(folders);
                }
            }
        }

        /// <summary>
        /// Lista los ficheros de un directorio en estricto orden por nombre.
        /// </summary>
        /// <param name="ruta"></param>
        /// <returns></returns>
        public static List<FileInfo> ListarFicherosPorNombre(string ruta, SearchOption opcion)
        {
            if (!Directory.Exists(ruta))
            {
                throw new DirectoryNotFoundException($"El directorio no existe: {ruta}");
            }
            DirectoryInfo di = new(ruta);
            return di.GetFiles("*", opcion).OrderBy(f => f.Name).ToList<FileInfo>();
        }

        /// <summary>
        /// Comprime un fichero con GZip. Tomado de la documentación de MS.
        /// </summary>
        /// <param name="fileToCompress"></param>
        public static void Compress(FileInfo fileToCompress)
        {
            using (FileStream originalFileStream = fileToCompress.OpenRead())
            {
                if ((File.GetAttributes(fileToCompress.FullName) &
                    FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
                {
                    using FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz");
                    using GZipStream compressionStream = new(compressedFileStream, CompressionMode.Compress);
                    originalFileStream.CopyTo(compressionStream);
                    _log.Debug($"Compressed: {fileToCompress.Name}");
                }
            }
        }

        /// <summary>
        /// Descomprime un fichero GZip. Tomado de la documentación de MS.
        /// </summary>
        /// <param name="fileToDecompress"></param>
        public static void Decompress(FileInfo fileToDecompress)
        {
            using FileStream originalFileStream = fileToDecompress.OpenRead();
            string currentFileName = fileToDecompress.FullName;
            string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

            using (FileStream decompressedFileStream = File.Create(newFileName))
            {
                using (GZipStream decompressionStream = new(originalFileStream, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(decompressedFileStream);
                    _log.Debug($"Decompressed: {fileToDecompress.Name}");
                    decompressionStream.Close();
                }
                decompressedFileStream.Close();
            }
            originalFileStream.Close();
        }

        public static FileInfo? ComprimirTarBz2(FileInfo fichero, params FileInfo[] ficheros)
        {
            string nombreFicheroComprimido = fichero.FullName.Remove(fichero.FullName.Length - fichero.Extension.Length) + ".tar.bz2";
            using (Stream stream = File.OpenWrite(nombreFicheroComprimido))
            {
                WriterOptions wo = new(CompressionType.BZip2)
                {
                    LeaveStreamOpen = false,
                };
                wo.ArchiveEncoding.Default = Encoding.Default;
                using IWriter writer = WriterFactory.Open(stream, ArchiveType.Tar, wo);
                writer.Write(fichero.Name, fichero);
                if (ficheros != null && ficheros.Length > 0)
                {
                    foreach (FileInfo fi in ficheros)
                    {
                        writer.Write(fi.Name, fi);
                    }
                }
            }
            FileInfo fiTarBz2 = new(nombreFicheroComprimido);
            return fiTarBz2.Exists ? fiTarBz2 : null;
        }

        public static List<FileInfo> DescomprimirTarBz2(FileInfo ficheroComprimido, string password)
        {
            List<FileInfo> ficheros = [];
            using (Stream stream = ficheroComprimido.OpenRead())
            {
                ReaderOptions? ro = String.IsNullOrEmpty(password) ?
                    null :
                    new ReaderOptions()
                    {
                        Password = password
                    };
                using IReader reader = ReaderFactory.Open(stream, ro);
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        string nombreDestino = $"{ficheroComprimido.DirectoryName}{Path.DirectorySeparatorChar}{reader.Entry.Key}";
                        reader.WriteEntryToFile(
                            nombreDestino,
                            new ExtractionOptions()
                            {
                                Overwrite = true
                            }
                        );
                        ficheros.Add(new FileInfo(nombreDestino));
                    }
                }
            }
            return ficheros;
        }

        public static List<FileInfo> DescomprimirTarBz2(FileInfo ficheroComprimido)
        {
            return DescomprimirTarBz2(ficheroComprimido, string.Empty);
        }

        public static FileInfo UnirFicheroFragmentado(FileInfo primerFichero)
        {
            string nombre = primerFichero.Name.Remove(primerFichero.Name.Length - primerFichero.Extension.Length);
            if (primerFichero.Directory == null)
            {
                throw new DirectoryNotFoundException($"No existe información del directorio del fichero {primerFichero.Name}");
            }
            FileInfo ficheroUnido = new(fileName: $"{primerFichero.Directory.FullName}{Path.DirectorySeparatorChar}{nombre}");
            using (FileStream fs = new(ficheroUnido.FullName, FileMode.Append))
            {
                int size = 0;
                foreach (FileInfo f in primerFichero.Directory.GetFiles($"{nombre}.*"))
                {
                    if (f.Name.Equals(ficheroUnido.Name))
                    {
                        continue;
                    }
                    byte[] bytes = File.ReadAllBytes(f.FullName);
                    fs.Write(bytes, 0, bytes.Length);
                    size += bytes.Length;
                }
            }
            return ficheroUnido;
        }

        public static string NormalizarRuta(string ruta, bool agregarSeparador)
        {
            _log.Trace($"Ruta sin normalizar: {ruta}");
            if (String.IsNullOrEmpty(ruta))
            {
                return ruta;
            }
            string rutaNormalizada = CmdHelper.SustituirTokensConVariablesDeEntorno(ruta);
            _log.Trace($"Ruta normalizada: {rutaNormalizada}");
            return agregarSeparador ? (!rutaNormalizada.EndsWith($"{Path.DirectorySeparatorChar}") ? $"{rutaNormalizada}{Path.DirectorySeparatorChar}" : rutaNormalizada) : rutaNormalizada;
        }

        public static string NormalizarRuta(string ruta)
        {
            return NormalizarRuta(ruta, true);
        }

    }
}
