using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SimpleBrowser.WebDriver;

namespace TestometrikaParser
{
    class Program
    {
        public static List<Test> Tests = new List<Test>();
        public static Test test = new Test();

        static void Main(string[] args)
        {
           

            Console.OutputEncoding = Encoding.UTF8;
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://testometrika.com/woman/how-much-are-you-in-love/");
            var description = driver.FindElement(By.ClassName("ts__description")).Text;
            Console.WriteLine("Description: {0}", description);
            test.description = description;

            Random rnd = new Random();

            var element = driver.FindElement(By.ClassName("ts__btn-bar"));
            element.Click();

            WaitForQuestion(driver, "0");

            var countOfTests = driver.FindElement(By.XPath("//h3[contains(@class, 'ts__progress')]")).Text;
            string regex = "/";
            var unparsedCountOfTests = Regex.Split(countOfTests, regex);

            for (int i = 0; i < int.Parse(unparsedCountOfTests[1]); i++)
            {
                WaitForQuestion(driver, i.ToString());
                OutputQuestionData(driver);
            }


            WaitForResult(driver);

            var resultBody = driver.FindElement(By.XPath("//div[contains(@class, 'result__body')]"));

            var resultList = resultBody.FindElement(By.ClassName("result__description"));

            var answers = resultList.FindElements(By.ClassName("result-stenayn"));

            foreach (var a in answers)
            {
                Console.WriteLine(a.Text);
            }
        }

        public static void OutputQuestionData(IWebDriver driver)
        {
            Random rnd = new Random();

            var question = driver.FindElement(By.XPath("//div[contains(@class, 'ts__background-color')]"));
            var q = question.FindElement(By.ClassName("ts__question-title"));

            var answer_list = question.FindElement(By.ClassName("ts__answer-list"));

            var answers = answer_list.FindElements(By.ClassName("ts__answer-li"));

            Console.WriteLine(q.Text);
            test.Questions.title = q.Text;

            foreach (var ans in answers)
            {
                Console.WriteLine(ans.Text);
                test.Questions.answer.answers.Add(ans.Text);
            }

            Console.WriteLine("Answer Count{0}", answers.Count);
            var rnd_answer = rnd.Next(0, answers.Count);

            var myAnswer = answers[rnd_answer];
            var clickableAnswer = myAnswer.FindElement(By.XPath(".//input[@type='radio']"));
            Console.WriteLine("My answer: #{0}, {1}", rnd_answer, myAnswer.Text);

            clickableAnswer.Click();

            GenerateJson();
        }

        public static void WaitForQuestion(IWebDriver driver, string numOfQuestion)
        {
            int timeout = 0;
            while (true)
            {
                try
                {
                    var isExists =
                        ExpectedConditions.ElementExists(By.XPath("//span[contains(@class, 'ts__progress-passed')]"));

                    var isNumberChanged =
                        ExpectedConditions.TextToBePresentInElement(
                            driver.FindElement(By.XPath("//span[contains(@class, 'ts__progress-passed')]")), numOfQuestion);

                    isExists(driver);

                    if (isNumberChanged(driver))
                    {
                        break;
                    }

                    if (timeout >= 10)
                    {
                        throw new Exception("We've been waiting too long");
                    }
                    Thread.Sleep(1000);
                    timeout++;
                }
                catch (Exception e)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public static void WaitForResult(IWebDriver driver)
        {
            int timeout = 0;
            while (true)
            {
                try
                {
                    var isExists =
                        ExpectedConditions.ElementExists(By.XPath("//h2[contains(@class, 'result__h2')]"));

                    isExists(driver);
                    
                    break;
                }
                catch (Exception e)
                {
                    if (timeout >= 10)
                    {
                        throw new Exception("We've been waiting too long");
                    }
                    Thread.Sleep(1000);
                    timeout++;
                }
            }
        }

        public static void GenerateJson()
        {
            var serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;

            string json = JsonConvert.SerializeObject(test);
            using (StreamWriter sw = new StreamWriter("Tests.txt"))
            {
                serializer.Serialize(sw, test);
            }
        }
    }
}
