using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using TestometrikaParser.JsonData;

namespace TestometrikaParser
{
    class Program
    {
        private static Test test;
        private static Way Way;
        private static TestScrapper testScrapper = new TestScrapper();

        private static IReadOnlyCollection<IWebElement> GetUrlsFromListOfTest()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless");
            IWebDriver driver = new ChromeDriver(options);

            driver.Navigate().GoToUrl("https://testometrika.com/business/?pc=100");

            var resultBody = driver.FindElement(By.XPath("//div[contains(@class, 'ajax-list')]"));
            var links = resultBody.FindElements(By.XPath(".//a[contains(@class, 'test-list__test ')]"));
            return links;
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var links = GetUrlsFromListOfTest();

            foreach (var url in links)
            {
                for (int i = 0; i < 3; ++i)
                {
                    Test_Startup(url.GetAttribute("href"));
                }
            }

            GenerateJson();
        }



        private static void Test_Startup(string testURL)
        {
            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("--headless");
            IWebDriver driver = new ChromeDriver(options);
            test = new Test();
            Way = new Way();

            driver.Navigate().Refresh();
            driver.Navigate().GoToUrl(testURL);
            Thread.Sleep(4000);
            var name = driver.FindElement(By.XPath("//h1[contains(@class, 'ts__h1')]")).Text;
            var description = driver.FindElement(By.ClassName("ts__description")).Text;

            Console.WriteLine("Name: {0}", name);
            Console.WriteLine("Description: {0}", description);

            test.Name = name;
            test.Description = description;

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


            var ResultText = "";

            foreach (var a in answers)
            {
                Console.WriteLine(a.Text);
                ResultText += a.Text;
            }

            int resultId = -1;

            for (int i = 0; i < test.Results.Count; ++i)
            {
                if (ResultText == test.Results[i].Text)
                {
                    resultId = i;
                    break;
                }
            }


            if (resultId == -1)
            {
                resultId = test.Results.Count + 1;
                test.Results.Add(resultId, new Result()
                {
                    Text = ResultText
                });
            }

            Way.ResultId = resultId;
            test.Ways.Add(Way);
            testScrapper.Tests.Add(test);

            driver.Close();

        }

        private static void OutputQuestionData(IWebDriver driver)
        {
            Random rnd = new Random();

            var question = driver.FindElement(By.XPath("//div[contains(@class, 'ts__background-color')]"));

            var q = driver.FindElement(By.XPath("//span[contains(@class, 'ts__question-text')]"));

            var questionClassNameWithId = question.FindElement(By.XPath("//div[contains(@class, 'ts__question')]")).GetAttribute("class");

            string regex = " ";
            var questionClassName = Regex.Split(questionClassNameWithId, regex);
            string pattern = "[^__]*$";
            int questionId = Int32.Parse(Regex.Match(questionClassName.Last(), pattern).Value);

            var answer_list = question.FindElement(By.ClassName("ts__answer-list"));

            var answers = answer_list.FindElements(By.ClassName("ts__answer-li"));

            Console.WriteLine($"Id: {questionId}, Question: {q.Text}");

            Questions questionToJson = new Questions();

            for (int i = 0; i < answers.Count; ++i)
            {
                Console.WriteLine(answers[i].Text);
                questionToJson.Answer.Add(i, answers[i].Text);
            }

            questionToJson.Title = q.Text;

            var rnd_answer = rnd.Next(0, answers.Count);

            test.Questions.Add(questionId, questionToJson);
            var myAnswer = answers[rnd_answer];
            Way.AnswerRoads.Add(new AnswerRoad()
            {
                QuestionId = questionId,
                AnswerId = rnd_answer
            });

            var clickableAnswer = myAnswer.FindElement(By.XPath(".//input[@type='radio']"));
            Console.WriteLine("My answer: #{0}, {1}", rnd_answer, myAnswer.Text);
            clickableAnswer.Click();
        }

        private static void WaitForQuestion(IWebDriver driver, string numOfQuestion)
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

        private static void WaitForResult(IWebDriver driver)
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

        private static void GenerateJson()
        {
            var serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter("Tests.txt"))
            {
                serializer.Serialize(sw, testScrapper);
            }
        }
    }
}
