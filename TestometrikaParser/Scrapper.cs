using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Microsoft.VisualBasic.CompilerServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace TestometrikaParser
{
    public class Scrapper
    {
        private ObservableCollection<EntityModel> _entries = new ObservableCollection<EntityModel>();

        public ObservableCollection<EntityModel> Entries
        {
            get { return _entries; }
            set { _entries = value; }
        }

        public void ScrapeData(string page)
        {
            //var driver = new ChromeDriver();
            //driver.Navigate().GoToUrl("https://www.google.com/");
            //var element = driver.FindElement(By.XPath("//*[@id=\"tsf\"]/div[2]/div/div[1]/div/div[1]/input"));
            //element.SendKeys("webshop");
            //element.Submit();

            var web = new HtmlWeb();
            var doc = web.Load(page);


            var Articles = doc.DocumentNode.SelectNodes("//*[@class='js__ts-start ts__btn-to-test button-dark button-global button-start']");

            foreach (var article in Articles)
            {
                var header = HttpUtility.HtmlDecode(article.InnerText);
                //var header =  HttpUtility.HtmlDecode(article.SelectSingleNode(".//li[@class = 'js__ts-start']").InnerText;
                Console.WriteLine(article.Attributes["href"].Value);
            }
        }
    }
}
