using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConverterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConverterController : ControllerBase
    {
        static Dictionary<string, string> History = new Dictionary<string, string>();
        // To do : Türkçe karakterlerin şifrelenmesi
        static public string ChangeTurkishCharacter(string text)
        {
            text = text.Replace("İ", "I");
            text = text.Replace("ı", "i");
            text = text.Replace("Ğ", "G");
            text = text.Replace("ğ", "g");
            text = text.Replace("Ö", "O");
            text = text.Replace("ö", "o");
            text = text.Replace("Ü", "U");
            text = text.Replace("ü", "u");
            text = text.Replace("Ş", "S");
            text = text.Replace("ş", "s");
            text = text.Replace("Ç", "C");
            text = text.Replace("ç", "c");
            return text;
        }

        // POST api/values
        [HttpPost]
        public void WebUrlToDeepLink([FromBody] string request)
        {
            string saltUrl = request.Replace("https://www.trendyol.com/", "");
            string responseDeeplinkUrl = "ty://?";
            if (saltUrl.Contains("-p-"))
            {
                responseDeeplinkUrl += "Page=Product";
                string contentId = saltUrl.Substring(saltUrl.IndexOf("-p-") + 3).Split('?').First();
                responseDeeplinkUrl += "&ContentId=" + contentId;


                if (saltUrl.Contains("?"))
                {
                    string detailPageUrl = saltUrl.Substring(saltUrl.IndexOf("?") + 1);
                    bool isBoutiqueId = detailPageUrl.Contains("boutiqueId");
                    bool isMerchantId = detailPageUrl.Contains("merchantId");
                    string boutiqueId = null;
                    string merchantId = null;
                    if (isBoutiqueId && isMerchantId)// Parametre olarak eklenmeli(ok)
                    {
                        var pageDetails = detailPageUrl.Split('&');
                        boutiqueId = pageDetails[0].Contains("boutiqueId") ? pageDetails[0].Replace("boutiqueId=", "") : pageDetails[1].Replace("boutiqueId=", "");
                        merchantId = pageDetails[1].Contains("merchantId") ? pageDetails[1].Replace("merchantId=", "") : pageDetails[0].Replace("merchantId=", "");
                        responseDeeplinkUrl += "&CampaignId=" + boutiqueId;
                        responseDeeplinkUrl += "&MerchantId=" + merchantId;
                    }
                    else if (isBoutiqueId)
                    {
                        boutiqueId = detailPageUrl.Replace("boutiqueId=", "");
                        responseDeeplinkUrl += "&CampaignId=" + boutiqueId;
                    }
                    else if (isMerchantId)
                    {
                        merchantId = detailPageUrl.Replace("merchantId=", "");
                        responseDeeplinkUrl += "&MerchantId" + merchantId;

                    }

                }
                History.Add(request, responseDeeplinkUrl);

            }
            else if (saltUrl.StartsWith("sr"))
            {
                responseDeeplinkUrl += "Page=Search";
                string query = saltUrl.Substring(saltUrl.IndexOf("sr") + 2).Replace("?q=", "");
                responseDeeplinkUrl += "&Query=" + ChangeTurkishCharacter(query);

                History.Add(request, responseDeeplinkUrl);

            }
            else
            {
                responseDeeplinkUrl += "Page=Home";

                History.Add(request, responseDeeplinkUrl);
            }
        }

        [HttpPost]
        public void DeepLinkToWebUrl([FromBody] string request)
        {
            string saltUrl = request.Replace("ty://?", "");
            string responseWebUrl = "https://www.trendyol.com/";
            if (saltUrl.Contains("Page=Product"))
            {
                responseWebUrl += "brand/name-p-";
                string contentId = saltUrl.Split('&')[1].Replace("ContentId=", "");
                responseWebUrl += contentId;
                if (saltUrl.Split('&').Length > 2)
                {
                    string detailPageUrl = saltUrl.Substring(saltUrl.Split('&')[0].Length + saltUrl.Split('&')[1].Length);
                    bool isCampaingId = detailPageUrl.Contains("CampaignId");
                    bool isMerchantId = detailPageUrl.Contains("MerchantId");
                    string campaingId = null;
                    string merchantId = null;
                    if (isCampaingId && isMerchantId)// Daha esnek olmalı
                    {
                        var pageDetails = detailPageUrl.Split('&');
                        campaingId = pageDetails[0].Contains("CampaignId") ? pageDetails[0].Replace("CampaignId=", "") : pageDetails[1].Replace("CampaignId=", "");
                        merchantId = pageDetails[1].Contains("MerchantId") ? pageDetails[1].Replace("MerchantId=", "") : pageDetails[0].Replace("MerchantId=", "");
                        responseWebUrl += "?boutiqueId=" + campaingId;
                        responseWebUrl += "&merchantId=" + merchantId;
                    }
                    else if (isCampaingId)
                    {
                        campaingId = detailPageUrl.Replace("CampaignId=", "");
                        responseWebUrl += "?boutiqueId=" + campaingId;
                    }
                    else if (isMerchantId)
                    {
                        merchantId = detailPageUrl.Replace("merchantId=", "");
                        responseWebUrl += "?merchantId" + merchantId;

                    }

                }
                History.Add(request, responseWebUrl);

            }
            else if (saltUrl.Contains("Page=Search"))
            {
                responseWebUrl += "sr";
                string query = saltUrl.Split('&')[1].Replace("Query=", "");
                responseWebUrl += "?q=" + ChangeTurkishCharacter(query);

                History.Add(request, responseWebUrl);

            }
            else
            {
                History.Add(request, responseWebUrl);
            }
        }


    }
}
