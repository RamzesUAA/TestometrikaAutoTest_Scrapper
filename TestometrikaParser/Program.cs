using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Serilog;
using TestometrikaParser.JsonData;

namespace TestometrikaParser
{
    class Program
    {

        private static TestScrapper testScrapper = new TestScrapper();

        private static List<string> GetUrlsFromListOfTest()
        {
            List<string> ListOfLinks = new List<string>()
            {
                "https://testometrika.com/personality-and-temper/?pc=130",
                "https://testometrika.com/temperament/?pc=20",
                "https://testometrika.com/society/?pc=70",
                "https://testometrika.com/diagnosis-of-abnormalities/?pc=40",
                "https://testometrika.com/depression-and-stress/?pc=40",
                "https://testometrika.com/intellectual/?pc=40",
                "https://testometrika.com/educational/?pc=250",
                "https://testometrika.com/business/?pc=100",
                "https://testometrika.com/funny/?pc=110",
                "https://testometrika.com/man/?pc=20",
                "https://testometrika.com/woman/?pc=100",
                "https://testometrika.com/love-and-family/?pc=70",
                "https://testometrika.com/for-children/?pc=30",
                "https://testometrika.com/health/?pc=30"
            };


            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("--headless");
            IWebDriver driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), options, TimeSpan.FromMinutes(3));
            List<string> listOfTests = new List<string>();
            foreach (var list in ListOfLinks)
            {
                driver.Navigate().GoToUrl(list);
                var resultBody = driver.FindElement(By.XPath("//div[contains(@class, 'ajax-list')]"));
                var links = resultBody.FindElements(By.XPath(".//a[contains(@class, 'test-list__test ')]"));

                Thread.Sleep(1000);

                foreach (var hrefElement in links)
                {
                    listOfTests.Add(hrefElement.GetAttribute("href"));
                }
            }

            return listOfTests;
        }

        static int first = -1;

        static void Main(string[] args)
        {
            var tasks = new List<Task>();
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logFile.log")
                .CreateLogger();


            Console.OutputEncoding = Encoding.UTF8;


       
            List<string> links = null;
            using (StreamReader file = File.OpenText("TestsLinks.txt"))
            {
                JsonSerializer deserializer = new JsonSerializer();
                links = (List<string>)deserializer.Deserialize(file, typeof(List<string>));
            }

            foreach (var l in links)
            {
                Console.WriteLine(l);
            }

            Console.WriteLine(links.Count);


            List<string> testLinks = links.Skip(1).Take(1).ToList();


            foreach (var url in testLinks)
            {
                Test test = new Test();
                first = -1;
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        Task task = new Task(() => Test_Startup(url, test));
                        tasks.Add(task);
                        task.Start();
                    }
                    Task.WaitAll(tasks.ToArray());
                }
            }


            Task.WaitAll(tasks.ToArray());
            GenerateJson();

            watch.Stop();

            //Output the milliseconds elapsed to the console.
            Console.WriteLine("Tasks take " + watch.ElapsedMilliseconds + " milliseconds");
        }



        private static void Test_Startup(Object testURL, Test test)
        {
            ChromeOptions options;
            //options.AddArgument("--headless");
            IWebDriver driver = null;

            Way way;

            try
            {
                options = new ChromeOptions();
                options.AddArgument("no-sandbox");
                //options.AddArgument("--headless");
                driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), options, TimeSpan.FromMinutes(3));


                way = new Way();

                driver.Navigate().Refresh();
                driver.Navigate().GoToUrl((string)testURL);
                Thread.Sleep(4000);
                var name = driver.FindElement(By.XPath("//h1[contains(@class, 'ts__h1')]")).Text;
                var description = driver.FindElement(By.ClassName("ts__description")).Text;

                Console.WriteLine("Name: {0}", name);
                Console.WriteLine("Description: {0}", description);

                test.Name = name;
                test.Description = description;


                var element = driver.FindElement(By.ClassName("ts__btn-bar"));
                element.Click();

                WaitForQuestion(driver, "0");


                var countOfTests = driver.FindElement(By.XPath("//h3[contains(@class, 'ts__progress')]")).Text;
                string regex = "/";
                var unparsedCountOfTests = Regex.Split(countOfTests, regex);

                for (int i = 0; i < int.Parse(unparsedCountOfTests[1]); i++)
                {
                    WaitForQuestion(driver, i.ToString());
                    OutputQuestionData(driver, test, way);
                }


                WaitForResult(driver);





                // Adding Result Blog
                var resultBlog = driver.FindElement(By.ClassName("result__description-footer"));

                ResultBlog blog = new ResultBlog();

                var blogTextsCollection = resultBlog.FindElements(By.XPath(".//p"));
                foreach (var text in blogTextsCollection)
                {
                    blog.Text += text.Text;
                }


                var resultBlogLinks = resultBlog.FindElements(By.XPath(".//a[contains(@href, '/blog/')]"));

                foreach (var resultBlogLink in resultBlogLinks)
                {
                    blog.ImagesList.Add(resultBlogLink.GetAttribute("href"));
                }



                var blogId = "";


                foreach (var b in testScrapper.Blogs)
                {
                    if (blog.Text == b.Value.Text)
                    {
                        blogId = b.Key;
                        break;
                    }
                }

                if (blogId == "")
                {
                    blogId = $"blog/{testScrapper.Blogs.Count}";
                    testScrapper.Blogs.Add(blogId, blog);
                }







                // Adding results

                var resultBody = driver.FindElement(By.XPath("//div[contains(@class, 'result__body')]"));

                var resultList = resultBody.FindElement(By.ClassName("result__description"));

                var answers = resultList.FindElements(By.ClassName("result-stenayn"));


                var ResultText = "";

                foreach (var a in answers)
                {
                    ResultText += a.Text;
                }


                var ResultId = -1;



                foreach (var b in test.Results)
                {
                    string str1 = b.Value.Text;
                    string str2 = ResultText;
                    if (str1.Equals(str2))
                    {
                        ResultId = b.Key;
                        break;
                    }
                }

                if (ResultId == -1)
                {
                    ResultId = test.Results.Count;
                    test.Results.Add(ResultId, new Result()
                    {
                        Text = ResultText,
                        BlogDescription = blogId
                    });
                }

                way.ResultId = ResultId;
                test.Ways.Add(way);
                if (first == -1)
                {
                    testScrapper.Tests.Add(test);
                    first = 0;
                }
            }
            catch (Exception exception)
            {
                Log.Information(exception, (string)testURL);
                if (driver != null)
                {
                    driver.Quit();

                }
                return;
            }

            driver.Quit();
        }

        private static void OutputQuestionData(IWebDriver driver, Test test, Way way)
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


            test.Questions.TryAdd(questionId, questionToJson);


            var myAnswer = answers[rnd_answer];
            way.AnswerRoads.Add(new AnswerRoad()
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
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" +
                              "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" +
                              "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" +
                              "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" +
                              "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            var serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter("Tests.txt"))
            {
                serializer.Serialize(sw, testScrapper);
            }

        }
    }
}
