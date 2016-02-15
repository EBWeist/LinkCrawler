﻿using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace LinkCrawler.Utils.Helpers
{
    public static class MarkupHelpers
    {
        private static List<string> GetAllUrlsFromHtmlDocument(string markup, string searchPattern, string attribute)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(markup);
            var nodes = htmlDoc.DocumentNode.SelectNodes(searchPattern);

            if (nodes == null || !nodes.Any())
                return new List<string>();

            return nodes.Select(x => x.GetAttributeValue(attribute, string.Empty).TrimEnd('/')).ToList();
        }

        public static List<string> GetAllUrlsFromMarkup(string markup, bool checkImageTags)
        {
            var linkUrls = GetAllUrlsFromHtmlDocument(markup, Constants.Html.LinkSearchPattern, Constants.Html.Href);
            if (checkImageTags)
            {
                var imgUrls = GetAllUrlsFromHtmlDocument(markup, Constants.Html.ImgSearchPattern, Constants.Html.Src);
                linkUrls.AddRange(imgUrls);
            }
            return linkUrls;
        }
        
        /// <summary>
        /// Get's a list of all urls in markup and tires to fix the urls that Restsharp will have a problem with 
        /// (i.e relative urls, urls with no sceme, mailto links..etc)
        /// </summary>
        /// <returns>List of urls that will work with restsharp for sending http get</returns>
        public static List<string> GetUrlListFromMarkup(string markup, bool checkImages, string baseUrl)
        {
            var urlList = GetAllUrlsFromMarkup(markup, checkImages);
            var correctUrlList = new List<string>();

            foreach (var url in urlList)
            {
                Uri parsedUri;
                if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out parsedUri)
                    || url.StartsWith(Constants.Html.Mailto)
                    || url.StartsWith(Constants.Html.Tel))
                    continue;

                if (parsedUri.IsAbsoluteUri)
                {
                    correctUrlList.Add(url);
                }
                else if (url.StartsWith("//"))
                {
                    var newUrl = string.Concat("http:", url);
                    correctUrlList.Add(newUrl);
                }
                else if (url.StartsWith("/"))
                {
                    var newUrl = string.Concat(baseUrl, url);
                    correctUrlList.Add(newUrl);
                }
            }
            return correctUrlList;
        }
    }
}
