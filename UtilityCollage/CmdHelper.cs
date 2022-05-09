using NLog;
using System;
using System.Diagnostics;
using System.IO;

namespace UtilityCollage
{
    public sealed class CmdHelper
    {
        private static readonly Logger _log = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Método para ejecutar un comando windows. Por ejemplo:
        /// Para ejecutar "DIR /w", la línea sería:
        /// <code>
        /// try {
        ///     CmdHelper.CmdDefExitCode("DIR /w");
        ///     //Similar a:
        ///     //CmdHelper.Cmd("DIR /w", 0);
        /// } catch (Exception ex) {
        ///     //Arroja una excepción si el valor de salida del
        ///     //comando es diferente del valor por defecto valido, es decir 0.
        /// }
        /// </code>
        /// En este caso concreto la ejecución real sería:
        /// <code>.\cmd.exe /c [comando]</code>
        /// </summary>
        /// <param name="comando">Comando a ejecutar.</param>
        public static void CmdDefExitCode(string comando)
        {
            Cmd(null, comando, null, 0);
        }

        /// <summary>
        /// Método para ejecutar un comando windows. Por ejemplo:
        /// Para ejecutar "DIR /w", la línea sería:
        /// <code>
        /// try {
        ///     CmdHelper.CmdDefExitCode("DIR", "/w");
        ///     //Similar a:
        ///     //CmdHelper.Cmd("DIR", "/w", 0);
        /// } catch (Exception ex) {
        ///     //Arroja una excepción si el valor de salida del
        ///     //comando es diferente del valor por defecto valido, es decir 0.
        /// }
        /// </code>
        /// </summary>
        /// <param name="comando">Comando a ejecutar.</param>
        /// <param name="parametros">Parámetros del comando.</param>
        public static void CmdDefExitCode(string comando, string parametros)
        {
            Cmd(null, comando, parametros, 0);
        }

        /// <summary>
        /// Método para ejecutar un comando windows. Por ejemplo:
        /// Para ejecutar "DIR /w" en el directorio "c:\tmp", la línea sería:
        /// <code>
        /// try {
        ///     CmdHelper.CmdDefExitCode("c:\\tmp", "DIR", "/w");
        ///     //Similar a:
        ///     //CmdHelper.Cmd("c:\\tmp", "DIR", "/w", 0);
        /// } catch (Exception ex) {
        ///     //Arroja una excepción si el valor de salida del
        ///     //comando es diferente del valor por defecto valido, es decir 0.
        /// }
        /// </code>
        /// </summary>
        /// <param name="workingDirectory">Directorio desde donde se ejecutará el comando.</param>
        /// <param name="comando">Comando a ejecutar.</param>
        /// <param name="parametros">Parámetros del comando.</param>
        public static void CmdDefExitCode(string workingDirectory, string comando, string parametros)
        {
            Cmd(workingDirectory, comando, parametros, 0);
        }

        /// <summary>
        /// Método para ejecutar un comando windows. Por ejemplo:
        /// Para ejecutar "DIR /w", la línea sería:
        /// <code>
        /// try {
        ///     CmdHelper.Cmd("DIR /w", 1, 2, 3);
        /// } catch (Exception ex) {
        ///     //Arroja una excepción si el valor de salida del comando es diferente de 1, 2 o 3.
        /// }
        /// </code>
        /// En este caso concreto la ejecución real sería:
        /// <code>.\cmd.exe /c [comando]</code>
        /// </summary>
        /// <param name="comando">Comando a ejecutar.</param>
        /// <param name="valorSalidaCorrecta">Primer valor de la lista de valores de salida considerados correctos (obligatorio)</param>
        /// <param name="valoresSalidaCorrecta">Listado de valores de salida considerados correctos (opcional)</param>
        public static void Cmd(string comando, int valorSalidaCorrecta, params int[] valoresSalidaCorrecta)
        {
            Cmd(null, comando, null, valorSalidaCorrecta, valoresSalidaCorrecta);
        }

        /// <summary>
        /// Método para ejecutar un comando windows. Por ejemplo:
        /// Para ejecutar "DIR /w", la línea sería:
        /// <code>
        /// try {
        ///     CmdHelper.Cmd("DIR", "/w", 1, 2, 3);
        /// } catch (Exception ex) {
        ///     //Arroja una excepción si el valor de salida del comando es diferente de 1, 2 o 3.
        /// }
        /// </code>
        /// </summary>
        /// <param name="comando">Comando a ejecutar.</param>
        /// <param name="parametros">Parámetros del comando.</param>
        /// <param name="valorSalidaCorrecta">Primer valor de la lista de valores de salida considerados correctos (obligatorio)</param>
        /// <param name="valoresSalidaCorrecta">Listado de valores de salida considerados correctos (opcional)</param>
        public static void Cmd(string comando, string parametros, int valorSalidaCorrecta, params int[] valoresSalidaCorrecta)
        {
            Cmd(null, comando, parametros, valorSalidaCorrecta, valoresSalidaCorrecta);
        }

        /// <summary>
        /// Método para ejecutar un comando windows. Por ejemplo:
        /// Para ejecutar "DIR /w" en el directorio "c:\tmp", la línea sería:
        /// <code>
        /// try {
        ///     CmdHelper.Cmd("c:\\tmp", "DIR", "/w", 1, 2, 3);
        /// } catch (Exception ex) {
        ///     //Arroja una excepción si el valor de salida del comando es diferente de 1, 2 o 3.
        /// }
        /// </code>
        /// </summary>
        /// <param name="workingDirectory">Directorio desde donde se ejecutará el comando.</param>
        /// <param name="comando">Comando a ejecutar.</param>
        /// <param name="parametros">Parámetros del comando.</param>
        /// <param name="valorSalidaCorrecta">Primer valor de la lista de valores de salida considerados correctos (obligatorio)</param>
        /// <param name="valoresSalidaCorrecta">Listado de valores de salida considerados correctos (opcional)</param>
        public static void Cmd(string workingDirectory, string comando, string parametros, int valorSalidaCorrecta, params int[] valoresSalidaCorrecta)
        {
            int codigoSalida = Cmd(workingDirectory, comando, parametros);
            if (((valoresSalidaCorrecta == null || valoresSalidaCorrecta.Length == 0) && codigoSalida != valorSalidaCorrecta) || (valoresSalidaCorrecta != null && valoresSalidaCorrecta.Length > 0 && !Array.Exists<int>(valoresSalidaCorrecta, valor => codigoSalida == valor)))
            {
                throw new Exception($"[CMD ERROR] Codigo de salida: {codigoSalida}");
            }
        }

        /// <summary>
        /// Método para ejecutar un comando windows. Por ejemplo:
        /// Para ejecutar "DIR /w", la línea sería:
        /// <code>int exitCode = CmdHelper.Cmd("DIR", "/w");</code>
        /// </summary>
        /// <param name="comando">Comando a ejecutar.</param>
        /// <param name="parametros">Parámetros del comando.</param>
        /// <returns>Código de salida del comando</returns>
        public static int Cmd(string comando, string parametros)
        {
            return Cmd(null, comando, parametros);
        }

        /// <summary>
        /// Método para ejecutar un comando windows. Por ejemplo:
        /// Para ejecutar "DIR /w", la línea sería:
        /// <code>int exitCode = CmdHelper.Cmd("DIR /w");</code>
        /// En este caso concreto la ejecución real sería:
        /// <code>.\cmd.exe /c [comando]</code>
        /// </summary>
        /// <param name="comando">Comando a ejecutar.</param>
        /// <returns>Código de salida del comando</returns>
        public static int Cmd(string comando)
        {
            return Cmd(null, comando, null);
        }

        /// <summary>
        /// Método para ejecutar un comando windows. Por ejemplo:
        /// Para ejecutar "DIR /w" en el directorio "c:\tmp", la línea sería:
        /// <code>int exitCode = CmdHelper.Cmd("c:\\tmp", "DIR", "/w");</code>
        /// </summary>
        /// <param name="workingDirectory">Directorio desde donde se ejecutará el comando.</param>
        /// <param name="comando">Comando a ejecutar.</param>
        /// <param name="parametros">Parámetros del comando.</param>
        /// <returns>Código de salida del comando</returns>
        public static int Cmd(string workingDirectory, string comando, string parametros)
        {
            bool parametrosNulos = String.IsNullOrEmpty(parametros);
            ProcessStartInfo procStartInfo = new ProcessStartInfo()
            {
                FileName = parametrosNulos ? "cmd.exe" : comando,
                Arguments = parametrosNulos ? $"/c {comando}" : parametros,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            _log.Debug($"[CMD] {procStartInfo.FileName} {procStartInfo.Arguments}");
            if (!String.IsNullOrEmpty(workingDirectory) && System.IO.Directory.Exists(workingDirectory))
            {
                procStartInfo.WorkingDirectory = workingDirectory;
            }
            using (Process process = Process.Start(procStartInfo))
            {
                process.OutputDataReceived += NLogOutputHandler;
                process.BeginOutputReadLine();
                using (StreamReader srError = process.StandardError)
                {
                    process.WaitForExit();
                    string error = srError.ReadToEnd();
                    if (!String.IsNullOrWhiteSpace(error))
                    {
                        _log.Error(error);
                    }
                }
                int ec = process.ExitCode;
                _log.Debug($"Exit code: {ec}");
                return ec;
            }
        }

        private static void NLogOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                _log.Debug(outLine.Data);
            }
        }

    }
}
