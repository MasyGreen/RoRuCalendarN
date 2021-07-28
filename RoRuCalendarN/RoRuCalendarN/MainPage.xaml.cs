using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace RoRuCalendarN
{
    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// Получаем отметившихся "Я еду"
        /// </summary>
        /// <param name="linkpost"></param>
        /// <returns></returns>
        private string GetUsersPost(string htmllink)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(htmllink);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            StreamReader sr = new StreamReader(response.GetResponseStream());
            string srhtml = sr.ReadToEnd();
            sr.Close();

            HtmlParser parser = new HtmlParser();
            List<string> memberlist = new List<string>();
            IHtmlDocument doc01 = parser.ParseDocument(srhtml);
            foreach (IElement el01 in doc01.QuerySelectorAll("table[class='tablebg']"))
            {
                IHtmlDocument doc02 = parser.ParseDocument(el01.InnerHtml);
                foreach (IElement el02 in doc02.QuerySelectorAll("td[class='row']"))
                {
                    IHtmlDocument doc03 = parser.ParseDocument(el02.InnerHtml);
                    foreach (IElement el03 in doc03.QuerySelectorAll("table>tbody>tr"))
                    {
                        IHtmlDocument doc04 = parser.ParseDocument(el03.InnerHtml);
                        foreach (IElement el04 in doc03.QuerySelectorAll("span[class='postbody']"))
                        {
                            IHtmlDocument doc05 = parser.ParseDocument(el04.InnerHtml);
                            foreach (IElement el05 in doc05.QuerySelectorAll("a").OfType<IHtmlAnchorElement>().ToList())
                            {
                                memberlist.Add($"{el05.InnerHtml}");
                            }
                            break;
                        }
                    }
                }
                break;
            }

            #region Массив с участниками
            int curindex = 0;
            string firstpart = string.Empty;
            string secondpart = string.Empty;
            foreach (string member in memberlist.Distinct())
            {
                curindex++;
                if (curindex < 5) firstpart += $"{member}, "; else secondpart += $"{member}, ";
            }
            #endregion

            string resulthtml = "";
            #region Генерация HTML с списком участников (5 первых и остальные hide-more)
            resulthtml = "<div>\n";
            resulthtml += $"{firstpart.Trim().TrimEnd(',')}\n";
            if (!string.IsNullOrEmpty(secondpart))
                resulthtml += $"<span class=\"more\"> еще...</span>\n<span class=\"expanding\">{secondpart.Trim().TrimEnd(',')}</span>\n";
            resulthtml += "</div>\n";
            #endregion

            return resulthtml;
        }

        /// <summary>
        /// Разбор страницы "Календарь покатушек"
        /// </summary>
        /// <returns></returns>
        private HtmlWebViewSource GetSourseHtml()
        {
            string htmllink = @"https://www.roller.ru/forum/pokatushki.php";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(htmllink);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1251));
            string srhtml = sr.ReadToEnd();
            sr.Close();

            HtmlParser parser = new HtmlParser();
            IHtmlDocument doc01 = parser.ParseDocument(srhtml);

            #region Начало страницы
            string resulthtml = "<html>\n" +
                "<style>\n" +
                "       body{font-family:Arial,Sans-serif;font-size:12px;color:#666666;}\n" +
                "       h1{font-size:20px;color:#E87400;text-align: center;}\n" +
                "       .month{font-size:14px;font-weight:bold;color:#666666;padding-top:5px;padding-bottom:20px;text-align: center;}\n" +
                "       .day{font-size:12px;font-weight:bold;color:#666666;}\n" +
                "       .holiday{font-size:12px;font-weight:bold;color:#E87400;}\n" +
                "       .event a, .event a:visited{color:#6c7f03;}\n" +
                "       .frm{color:#E87400;text-align: center;}\n" +
                "       .crop{overflow: hidden;white-space:nowrap;text-overflow: ellipsis;width: 150px;}\n" +
                "       .more{display:block;}\n" +
                "       .expanding{display: none;}\n" +
                "       div:hover>.expanding{display:block;}\n" +
                "       div:hover>.more{display: none;}\n" +
                "       .member{font-size: 12px;font-style: italic;color:gray;}\n" +
                "       .linebreak {border-bottom:1px solid lightgray;}\n" +
                "    </style>\n" +
                "<body><h1>Календарь покатушек</h1>\n";
            resulthtml += "<table border=\"0\" cellpadding=\"5\" cellspacing=\"0\"><tbody>\n";
            #endregion

            foreach (IElement el01 in doc01.QuerySelectorAll("div[class='content']"))
            {
                IHtmlDocument doc02 = parser.ParseDocument(el01.InnerHtml);
                bool isadd = true;
                foreach (IElement el02 in doc02.QuerySelectorAll("tr"))
                {
                    if (el02.InnerHtml.ToUpper().Contains("ДАЛЕКОЕ БУДУЩЕЕ")
                        || el02.InnerHtml.ToUpper().Contains("ПРОШЕДШИЕ ПОКАТУШКИ")
                        || el02.InnerHtml.ToUpper().Contains("ДОБАВИТЬ ПОКАТУШКУ"))
                        isadd = false;

                    if (isadd)
                    {
                        string curhtml = el02.InnerHtml.Replace("/forum/viewtopic.php?t=", "https://www.roller.ru/forum/viewtopic.php?t=");

                        #region Сокращение дат
                        curhtml = curhtml.Replace("/ понедельник</div>", "/ Пн</div>");
                        curhtml = curhtml.Replace("/ вторник</div>", "/ Вт</div>");
                        curhtml = curhtml.Replace("/ среда</div>", "/ Ср</div>");
                        curhtml = curhtml.Replace("/ четверг</div>", "/ Чт</div>");
                        curhtml = curhtml.Replace("/ пятница</div>", "/ Пт</div>");
                        curhtml = curhtml.Replace("/ суббота</div>", "/ Сб</div>");
                        curhtml = curhtml.Replace("/ воскресенье</div>", "/ Вс</div>");
                        #endregion

                        resulthtml += $"<tr valign=\"top\">{curhtml}</tr>\n";

                        #region Список участников
                        string membrrhtml = string.Empty;
                        if (curhtml.Contains("viewtopic.php"))
                        {
                            IHtmlDocument topiclist = parser.ParseDocument(curhtml);
                            foreach (IElement topic in topiclist.QuerySelectorAll("a").OfType<IHtmlAnchorElement>().ToList())
                            {
                                string topiclink = ((IHtmlAnchorElement)topic).Href;
                                membrrhtml = GetUsersPost(topiclink);
                            }
                        }
                        if (!string.IsNullOrEmpty(membrrhtml))
                            resulthtml += $"<tr valign=\"top\"><td colspan=\"2\"></td><td class=\"member\">{membrrhtml}</td></tr>";
                        #endregion

                        //Разделитель
                        if (curhtml.Contains("viewtopic.php"))
                            resulthtml += "<tr valign=\"top\"><td colspan=\"3\" class=\"linebreak\"></td></tr>\n";
                    }
                }
            }

            resulthtml += "<tr valign=\"top\"><td colspan=\"3\" class=\"frm\"><a href=\"https://www.roller.ru/forum/\">Форум. Главная страница</a></td></tr>\n";
            resulthtml += "<tr valign=\"top\"><td colspan=\"3\" class=\"line -break\"></td></tr>\n";
            resulthtml += "<tr valign=\"top\"><td colspan=\"3\" class=\"frm\">masygreen &copy; 2021</td></tr>\n";
            resulthtml += "</tbody></table></body></html>";

            HtmlWebViewSource htmlwebviewsource = new HtmlWebViewSource { Html = resulthtml };

            return htmlwebviewsource;
        }

        /// <summary>
        /// Нажатие кнопки "Назад"
        /// </summary>
        /// <returns></returns>
        protected override bool OnBackButtonPressed()
        {
            base.OnBackButtonPressed();
            if (webView.CanGoBack)
            {
                webView.GoBack();
                return true;
            }
            else
            {
                base.OnBackButtonPressed();
                return true;
            }
        }

        public MainPage()
        {
            InitializeComponent();

            webView = new WebView
            {
                Source = GetSourseHtml(),
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            ToolbarItems.Add(new ToolbarItem("Instagram", "instagram.png", () => { Browser.OpenAsync("https://www.instagram.com/roller.ru", BrowserLaunchMode.SystemPreferred); }));
            ToolbarItems.Add(new ToolbarItem("Telegram", "telegram.png", () => { Browser.OpenAsync("https://t.me/mskrollerru", BrowserLaunchMode.SystemPreferred); }));
            ToolbarItems.Add(new ToolbarItem("VK", "vk.png", () => { Browser.OpenAsync("https://vk.com/mskroller ", BrowserLaunchMode.SystemPreferred); }));

            ToolbarItems.Add(new ToolbarItem("|", "", () => { }));

            ToolbarItems.Add(new ToolbarItem("Обновить", "", () => { webView.Source = GetSourseHtml(); }));
            ToolbarItems.Add(new ToolbarItem("Назад", "", () => { webView.GoBack(); }));

            Content = new StackLayout { Children = { webView } };
        }
    }
}
