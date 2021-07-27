using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
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
            string silehtml = sr.ReadToEnd();
            sr.Close();

            HtmlParser parser = new HtmlParser();
            IHtmlDocument document = parser.ParseDocument(silehtml);
            string newhtml = "";
            foreach (IElement el in document.QuerySelectorAll("table[class='tablebg']"))
            {
                IHtmlDocument subdocument = parser.ParseDocument(el.InnerHtml);
                foreach (IElement subel in subdocument.QuerySelectorAll("td[class='row']"))
                {
                    IHtmlDocument tr = parser.ParseDocument(subel.InnerHtml);
                    foreach (IElement trel in tr.QuerySelectorAll("table>tbody>tr"))
                    {
                        IHtmlDocument td = parser.ParseDocument(trel.InnerHtml);
                        foreach (IElement tdel in tr.QuerySelectorAll("span[class='postbody']"))
                        {
                            newhtml = "";
                            IHtmlDocument user = parser.ParseDocument(tdel.InnerHtml);
                            foreach (IElement uel in user.QuerySelectorAll("a").OfType<IHtmlAnchorElement>().ToList())
                            {
                                newhtml += $"{uel.InnerHtml}; ";
                            }

                            break;
                        }
                    }
                }
                break;
            }
            return newhtml;
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
            string silehtml = sr.ReadToEnd();
            sr.Close();

            HtmlParser parser = new HtmlParser();
            IHtmlDocument document = parser.ParseDocument(silehtml);

            string newhtml = "<html>\n" +
                "<style>\n" +
                "        body{font-family:Arial,Sans-serif;font-size:12px;color:#666666;}\n" +
                "        h1{font-size:20px;color:#E87400;text-align: center;}\n" +
                "        .month{font-size:14px;font-weight:bold;color:#666666;padding-top:5px;padding-bottom:20px;text-align: center;}\n" +
                "        .day{font-size:12px;font-weight:bold;color:#666666;}\n" +
                "        .holiday{font-size:12px;font-weight:bold;color:#E87400;}\n" +
                "        .event a, .event a:visited{color:#6c7f03;}\n" +
                "        .frm{color:#E87400;text-align: center;}\n" +
                "    </style>\n" +
                "<body><h1>Календарь покатушек</h1>\n";
            newhtml += "<table border=\"0\" cellpadding=\"5\" cellspacing=\"0\"><tbody>\n";

            foreach (IElement el in document.QuerySelectorAll("div[class='content']"))
            {
                IHtmlDocument subdocument = parser.ParseDocument(el.InnerHtml);
                bool isadd = true;
                foreach (IElement subel in subdocument.QuerySelectorAll("tr"))
                {
                    if (subel.InnerHtml.ToUpper().Contains("ДАЛЕКОЕ БУДУЩЕЕ")
                        || subel.InnerHtml.ToUpper().Contains("ПРОШЕДШИЕ ПОКАТУШКИ")
                        || subel.InnerHtml.ToUpper().Contains("ДОБАВИТЬ ПОКАТУШКУ"))
                        isadd = false;

                    if (isadd)
                    {
                        string curhtml = subel.InnerHtml.Replace("/forum/viewtopic.php?t=", "https://www.roller.ru/forum/viewtopic.php?t=");

                        #region Список участников
                        string usershtml = string.Empty;
                        if (curhtml.Contains("viewtopic.php"))
                        {
                            IHtmlDocument ldocument = parser.ParseDocument(curhtml);
                            foreach (IElement lel in ldocument.QuerySelectorAll("a").OfType<IHtmlAnchorElement>().ToList())
                            {
                                string linkpost = ((IHtmlAnchorElement)lel).Href;
                                usershtml = GetUsersPost(linkpost);
                            }
                        }
                        #endregion

                        curhtml = curhtml.Replace("/ понедельник</div>", "/ Пн</div>");
                        curhtml = curhtml.Replace("/ вторник</div>", "/ Вт</div>");
                        curhtml = curhtml.Replace("/ среда</div>", "/ Ср</div>");
                        curhtml = curhtml.Replace("/ четверг</div>", "/ Чт</div>");
                        curhtml = curhtml.Replace("/ пятница</div>", "/ Пт</div>");
                        curhtml = curhtml.Replace("/ суббота</div>", "/ Сб</div>");
                        curhtml = curhtml.Replace("/ воскресенье</div>", "/ Вс</div>");

                        newhtml += $"<tr valign=\"top\">{curhtml}</tr>\n";

                        if(!string.IsNullOrEmpty(usershtml))
                        newhtml += $"<tr valign=\"top\"><td colspan=\"2\"></td><td class=\"event\">{usershtml}</td></tr>";
                    }
                }
            }
            newhtml += "<tr valign=\"top\"><td colspan=\"3\" class=\"frm\"><a href=\"https://www.roller.ru/forum/\">Форум</a></td></tr>\n";
            newhtml += "</tbody></table></body></html>";

            HtmlWebViewSource htmlwebviewsource = new HtmlWebViewSource { Html = newhtml };

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

            ToolbarItems.Add(new ToolbarItem("Обновить", "", () => { webView.Source = GetSourseHtml(); }));
            ToolbarItems.Add(new ToolbarItem("Назад", "", () => { webView.GoBack(); }));

            Content = new StackLayout { Children = { webView } };
        }
    }
}
