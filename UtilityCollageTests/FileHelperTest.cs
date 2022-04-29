﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UtilityCollageTests
{
    public class FileHelperTest
    {
        [Fact]
        public void EsRutaRelativaTest()
        {
            Assert.True(FileHelper.EsRutaRelativa("../"));
            Assert.False(FileHelper.EsRutaRelativa("c:\\Windows"));
        }

        [Fact]
        public void CrearEliminarDirectorioTest()
        {
            int numeroAleatorio = new Random().Next();
            string rutaDirectorio = FileHelper.NormalizarRuta($"%TMP%{Path.DirectorySeparatorChar}test_{numeroAleatorio}");
            FileHelper.CrearDirectorio(rutaDirectorio);
            Assert.True(Directory.Exists(rutaDirectorio));
            FileHelper.EliminarDirectorio(rutaDirectorio);
            Assert.False(Directory.Exists(rutaDirectorio));
        }
    }
}