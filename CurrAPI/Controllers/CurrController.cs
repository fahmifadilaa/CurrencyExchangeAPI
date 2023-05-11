using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Globalization;


namespace CurrAPI.Controllers
{
    public class CurrController : ApiController
    {

        [HttpGet]
        [ActionName("GetCurrencyRateInEuro")]
        public  float GetCurrencyRateInEuro(string currency)
            {
                if (currency.ToLower() == "")
                    throw new ArgumentException("Invalid Argument! currency parameter cannot be empty!");
                if (currency.ToLower() == "eur")
                    throw new ArgumentException("Invalid Argument! Cannot get exchange rate from EURO to EURO");

                try
                {

                    string rssUrl = string.Concat("http://www.ecb.int/rss/fxref-", currency.ToLower() + ".html");

                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    doc.Load(rssUrl);

                    System.Xml.XmlNamespaceManager nsmgr = new System.Xml.XmlNamespaceManager(doc.NameTable);
                    nsmgr.AddNamespace("rdf", "http://purl.org/rss/1.0/");
                    nsmgr.AddNamespace("cb", "http://www.cbwiki.net/wiki/index.php/Specification_1.1");

                    System.Xml.XmlNodeList nodeList = doc.SelectNodes("//rdf:item", nsmgr);

                    foreach (System.Xml.XmlNode node in nodeList)
                    {
                        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                        ci.NumberFormat.CurrencyDecimalSeparator = ".";

                        try
                        {
                            float exchangeRate = float.Parse(
                                node.SelectSingleNode("//cb:statistics//cb:exchangeRate//cb:value", nsmgr).InnerText,
                                NumberStyles.Any,
                                ci);

                            return exchangeRate;
                        }
                        catch { }
                    }

                    return 0;
                }
                catch
                {
                    return 0;
                }
            }

        [HttpGet]
        [ActionName("GetExchangeRate")]
        public HttpResponseMessage GetExchangeRate(string from, string to, float amount)
            {
                if (from == null || to == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest,0);

                if (from.ToLower() == "eur" && to.ToLower() == "eur")
                return Request.CreateResponse(HttpStatusCode.OK,amount);

                if (amount == 0 )
                return Request.CreateResponse(HttpStatusCode.BadRequest, 0);

            try
            {
                    float toRate = GetCurrencyRateInEuro(to);
                    float fromRate = GetCurrencyRateInEuro(from);

                    if (from.ToLower() == "eur")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, (amount * toRate));
                    }
                    else if (to.ToLower() == "eur")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, (amount / fromRate));
                    }
                    else
                    {
                        var res = (amount * toRate) / fromRate;
                    var  result = Request.CreateResponse(HttpStatusCode.OK, amount + " " + from +  " = " + res.ToString("0.##") + " " + to);
                    return result;
                    }
                }
                catch { return Request.CreateResponse(HttpStatusCode.BadRequest, 0); }
            }
        }
 }