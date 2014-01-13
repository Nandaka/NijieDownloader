﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NijieDownloader.Library;
using NijieDownloader.Library.Model;
using Nandaka.Common;

namespace NijieDownloaderConsole
{
    class Program
    {
        private static Nijie bot = new Nijie();

        static void Main(string[] args)
        {
            string username = "c894814@rmqkr.net";
            string password = "bugmenot";

            NijieLoginInfo info = bot.PrepareLoginInfo(username, password);

            var result = bot.DoLogin(info);
            Console.WriteLine("DoLogin: " + result);
            {
                var image = bot.ParseImage(70240);
                Console.WriteLine("Title: " + image.Title);
                Console.WriteLine("Desc: " + image.Description);
                Console.WriteLine("WorkDate: " + image.WorkDate);
                Console.WriteLine("Medium Url: " + image.MediumImageUrl);
                if (!image.IsManga)
                {
                    Console.WriteLine("Big Url: " + image.BigImageUrl);
                }
                else
                {
                    Console.WriteLine("Manga Urls: ");
                    foreach (var url in image.ImageUrls)
                        Console.WriteLine("- " + url);
                }
                Console.WriteLine("Tags: " + String.Join(", ", image.Tags));
                Console.WriteLine();
                Console.WriteLine();
            }
            {
                var image2 = bot.ParseImage(66994);
                Console.WriteLine("Title: " + image2.Title);
                Console.WriteLine("Desc: " + image2.Description);
                Console.WriteLine("WorkDate: " + image2.WorkDate);
                Console.WriteLine("Medium Url: " + image2.MediumImageUrl);
                if (!image2.IsManga)
                {
                    Console.WriteLine("Big Url: " + image2.BigImageUrl);
                }
                else
                {
                    Console.WriteLine("Manga Urls: ");
                    foreach (var url in image2.ImageUrls)
                        Console.WriteLine("- " + url);
                }
                Console.WriteLine("Tags: " + String.Join(", ", image2.Tags));
                Console.WriteLine();
                Console.WriteLine();
            }
            {
                var member = bot.ParseMember(40208);
                Console.WriteLine("UserName: " + member.UserName);
                Console.WriteLine("AvatarUrl: " + member.AvatarUrl);
                Console.WriteLine("Image Count: " + member.Images.Count);

                foreach (var imageData in member.Images)
                {
                    bot.ParseImage(imageData);
                    Console.WriteLine("- ImageId: " + imageData.ImageId);
                    Console.WriteLine("- Title: " + imageData.Title);
                    Console.WriteLine("- Desc: " + imageData.Description);
                    Console.WriteLine("- WorkDate: " + imageData.WorkDate);
                    Console.WriteLine("- Thumb: " + imageData.ThumbImageUrl);
                    Console.WriteLine("- Medium Url: " + imageData.MediumImageUrl);
                    Console.WriteLine("- IsManga: " + imageData.IsManga);

                    var filename = String.Format("{0} - {1} - {2} - {3}", member.MemberId, imageData.ImageId, imageData.Title, String.Join(" ", imageData.Tags));
                    if (imageData.IsManga)
                    {
                        for (int i = 0; i < imageData.ImageUrls.Count; ++i)
                        {
                            Console.WriteLine("Downloading: " + imageData.ImageUrls[i]);
                            filename = filename + "_p" + i + "." + Helper.ParseExtension(imageData.ImageUrls[i]);
                            filename = Helper.SanitizeFilename(filename);
                            bot.Download(imageData.ImageUrls[i], imageData.Referer, filename);
                            Console.WriteLine("Saving to: " + filename);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Downloading: " + imageData.BigImageUrl);
                        filename = filename + "." + Helper.ParseExtension(imageData.BigImageUrl);
                        filename = Helper.SanitizeFilename(filename);
                        bot.Download(imageData.BigImageUrl, imageData.Referer, filename);
                        Console.WriteLine("Saving to: " + filename);
                    }

                    Console.WriteLine();
                }
            }


            Console.WriteLine("END");
            Console.ReadLine();

        }
    }
}
