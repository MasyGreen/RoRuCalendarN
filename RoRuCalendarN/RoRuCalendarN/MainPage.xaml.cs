using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
        /// Получение страницы по ссылке
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        private string GetSourceHTMLFromLink(string link, bool is1251 = false)
        {
            string resultrhtml;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                StreamReader sr =
                    new(response.GetResponseStream(), is1251 ? Encoding.GetEncoding(1251) : Encoding.Default);
                resultrhtml = sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception ex)
            {
                resultrhtml = $"<h1>ERROR: {ex}</h1>";
            }
            return resultrhtml;
        }

        /// <summary>
        /// Разбор страницы поста, получение - "Я еду"
        /// </summary>
        /// <param name="linkpost"></param>
        /// <returns></returns>
        public string ParsePostPage(string pagehtml)
        {
            List<string> memberlist = new();

            if (!string.IsNullOrEmpty(pagehtml))
            {
                HtmlParser parser = new();
                IHtmlDocument doc01 = parser.ParseDocument(pagehtml);
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
                                foreach (IElement el05 in doc05.QuerySelectorAll("a").OfType<IHtmlAnchorElement>()
                                    .ToList())
                                {
                                    memberlist.Add($"{el05.InnerHtml}");
                                }

                                break;
                            }
                        }
                    }
                    break;
                }
            }

            if (memberlist.Count > 0)
            {
                #region Массив с участниками
                int curindex = 0;
                string firstpart = string.Empty;
                string secondpart = string.Empty;
                foreach (string member in memberlist.Distinct())
                {
                    curindex++;
                    if (curindex < 5) firstpart += $"{member}, ";
                    else secondpart += $"{member}, ";
                }
                #endregion

                #region Генерация HTML с списком участников (5 первых и остальные hide-more)
                string resulthtml = "<div>\n";
                resulthtml += $"{firstpart.Trim().TrimEnd(',')}\n";
                if (!string.IsNullOrEmpty(secondpart))
                    resulthtml +=
                        $"<span class=\"more\"> еще...</span>\n<span class=\"expanding\">{secondpart.Trim().TrimEnd(',')}</span>\n";
                resulthtml += "</div>\n";
                #endregion
                return resulthtml;
            }
            else return string.Empty;
        }

        /// <summary>
        /// Разбор страницы "Календарь покатушек"
        /// </summary>
        /// <returns></returns>
        public string ParseMainPage(string pagehtml, bool isaddmemberr)
        {
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
            #endregion
            resulthtml += "<table border=\"0\" cellpadding=\"5\" cellspacing=\"0\"><tbody>\n";

            if (!string.IsNullOrEmpty(pagehtml))
            {
                try
                {
                    HtmlParser parser = new();
                    IHtmlDocument doc01 = parser.ParseDocument(pagehtml);

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
                                string curhtml = el02.InnerHtml;//запись в календаре за день 

                                #region Сокращение дат
                                curhtml = curhtml.Replace("/ понедельник</div>", "/ Пн</div>");
                                curhtml = curhtml.Replace("/ вторник</div>", "/ Вт</div>");
                                curhtml = curhtml.Replace("/ среда</div>", "/ Ср</div>");
                                curhtml = curhtml.Replace("/ четверг</div>", "/ Чт</div>");
                                curhtml = curhtml.Replace("/ пятница</div>", "/ Пт</div>");
                                curhtml = curhtml.Replace("/ суббота</div>", "/ Сб</div>");
                                curhtml = curhtml.Replace("/ воскресенье</div>", "/ Вс</div>");
                                #endregion

                                string membrrhtml = string.Empty;
                                if (!curhtml.Contains("viewtopic.php"))
                                {
                                    resulthtml += $"<tr valign=\"top\">{curhtml}</tr>\n";//это заголовок
                                }
                                else
                                {
                                    #region Список покатушек за день
                                    curhtml = curhtml.Replace("/forum/viewtopic.php?t=", "https://www.roller.ru/forum/viewtopic.php?t=");

                                    IHtmlDocument daylist = parser.ParseDocument($"<table><tr>{curhtml}</tr></table>");
                                    //Получаем первый столбец
                                    int ind = 0;
                                    string td01 = "";
                                    foreach (IElement tditem in daylist.QuerySelectorAll("tr > td"))
                                    {
                                        ind++;
                                        if (ind == 1)
                                            td01 = tditem.InnerHtml;
                                        if (ind == 3)
                                        {

                                            int indinday = 0;
                                            IHtmlDocument postlist = parser.ParseDocument(tditem.InnerHtml);
                                            foreach (IElement postitem in postlist.QuerySelectorAll("div[class='event']"))
                                            {
                                                //nowrap - запрет переноса на новую строку для даты (только один раз для вывода)
                                                indinday++;
                                                resulthtml += $"<tr valign=\"top\"><td nowrap>{(indinday == 1 ? td01 : "")}</td><td></td><td>{postitem.InnerHtml}</td></tr>";

                                                foreach (IElement topic in postitem.QuerySelectorAll("a").OfType<IHtmlAnchorElement>()
                                                    .ToList())
                                                {
                                                    string topiclink = ((IHtmlAnchorElement)topic).Href;
                                                    string topichtml = GetSourceHTMLFromLink(topiclink);

                                                    if (isaddmemberr)
                                                    {
                                                        membrrhtml = ParsePostPage(topichtml);
                                                        if (!string.IsNullOrEmpty(membrrhtml))
                                                            resulthtml +=
                                                                $"<tr valign=\"top\"><td colspan=\"2\"></td><td class=\"member\">{membrrhtml}</td></tr>";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

                                //Разделитель
                                if (curhtml.Contains("viewtopic.php"))
                                    resulthtml += "<tr valign=\"top\"><td colspan=\"3\" class=\"linebreak\"></td></tr>\n";
                            }
                        }
                    }
                }
                catch (Exception ex) { resulthtml += $"<tr valign=\"top\"><td colspan=\"3\" class=\"frm\">Ошибка: {ex}</td></tr>\n"; }
            }
            else { resulthtml += "<tr valign=\"top\"><td colspan=\"3\" class=\"frm\">Что-то пошло не так...</td></tr>\n"; }

            resulthtml += "<tr valign=\"top\"><td colspan=\"3\" class=\"line -break\"></td></tr>\n";
            resulthtml += "<tr valign=\"top\"><td colspan=\"3\" class=\"frm\">masygreen &copy; 2021</td></tr>\n";
            resulthtml += "</tbody></table></body></html>";

            return resulthtml;
        }

        /// <summary>
        /// Подготовка файла основного интерфейса
        /// </summary>
        /// <returns></returns>
        private HtmlWebViewSource GetSourseHtml()
        {
            string srhtml = GetSourceHTMLFromLink(@"https://www.roller.ru/forum/pokatushki.php", true);
            string resulthtml = ParseMainPage(srhtml, true);
            HtmlWebViewSource htmlwebviewsource = new() { Html = resulthtml };
            return htmlwebviewsource;
        }

        private async Task<bool> LoadData()
        {
            webView.Source = GetSourseHtml();
            await Navigation.PopAsync();
            return true;
        }

        public MainPage()
        {
            InitializeComponent();

            webView = new WebView { VerticalOptions = LayoutOptions.FillAndExpand };

            curversoin.Text = $"{AppInfo.VersionString}";

            ToolbarItems.Add(new ToolbarItem("roru", "roru.png", () => { Browser.OpenAsync("https://www.roller.ru/forum", BrowserLaunchMode.SystemPreferred); }));
            ToolbarItems.Add(new ToolbarItem("Instagram", "instagram.png", () => { Browser.OpenAsync("https://www.instagram.com/roller.ru", BrowserLaunchMode.SystemPreferred); }));
            ToolbarItems.Add(new ToolbarItem("Telegram", "telegram.png", () => { Browser.OpenAsync("https://t.me/mskrollerru", BrowserLaunchMode.SystemPreferred); }));
            ToolbarItems.Add(new ToolbarItem("VK", "vk.png", () => { Browser.OpenAsync("https://vk.com/mskroller ", BrowserLaunchMode.SystemPreferred); }));

            ToolbarItems.Add(new ToolbarItem("|", "", () => { }));

            ToolbarItems.Add(new ToolbarItem("Refresh", "refresh.png", () =>
            {
                Task.Factory.StartNew(() =>
                {
                    Device.InvokeOnMainThreadAsync(async () => { await LoadData(); });
                });

            }));

            ToolbarItems.Add(new ToolbarItem("Back", "back.png", () => { webView.GoBack(); }));

            Content = new StackLayout { Children = { webView } };
        }

        /// <summary>
        /// Запуск первичный
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadData();
        }

        /// <summary>
        /// Нажатие кнопки "Назад"
        /// </summary>
        /// <returns></returns>
        protected override bool OnBackButtonPressed()
        {
            base.OnBackButtonPressed();
            if (webView.CanGoBack) { webView.GoBack(); } else { base.OnBackButtonPressed(); }
            return true;
        }
    }
}
