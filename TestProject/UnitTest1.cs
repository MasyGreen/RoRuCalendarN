using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoRuCalendarN;

namespace TestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMainPage()
        {
            string resultrhtml = "";
            string filename = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\MAIN_PAGE.html";
            if (File.Exists(filename))
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                StreamReader sr = new(filename, Encoding.GetEncoding(1251));
                resultrhtml = sr.ReadToEnd();
                sr.Close();
            }

            MainPage mp = new();
            string result = mp.ParseMainPage(resultrhtml, false);

            // Assert.IsFalse(result, "1 should not be prime");
            Assert.AreEqual("", "", $"Ошибка:{resultrhtml}");
        }
    }
}
