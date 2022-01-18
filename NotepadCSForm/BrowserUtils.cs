using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotepadCSForm
{
    internal static class BrowserUtils
    {
        private const string GithubUrl = "https://github.com";
        private const string GoogleUrl = "https://google.com";
        private const string YoutubeUrl = "https://youtube.com";
        private const string RedditUrl = "https://reddit.com";
        private const string InstagramUrl = "https://instagram.com";
        private const string TwitterUrl = "https://twitter.com";
        private const string FacebookUrl = "https://facebook.com";

        private static void OpenInBrowser(string url)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        public static void SearchOnStackOverflow(string query)
        {
            query = query.Replace(' ', '+');
            string searchUrl = $"{GoogleUrl}/search?q=site%3Astackoverflow.com+{query}";

            OpenInBrowser(searchUrl);
        }

        public static void OpenGithub()
        {
            OpenInBrowser(GithubUrl);
        }

        public static void OpenGithubProfile(string username)
        {
            OpenInBrowser($"{GithubUrl}/{username}");
        }

        public static void OpenGoogle()
        {
            OpenInBrowser(GoogleUrl);
        }

        public static void OpenYoutube()
        {
            OpenInBrowser(YoutubeUrl);
        }

        public static void OpenInstagram()
        {
            OpenInBrowser(InstagramUrl);
        }

        public static void OpenFacebook()
        {
            OpenInBrowser(FacebookUrl);
        }

        public static void OpenReddit()
        {
            OpenInBrowser(RedditUrl);
        }

        public static void OpenRedditProgrammerHumor()
        {
            OpenInBrowser($"{RedditUrl}/r/ProgrammerHumor");
        }

        public static void OpenRedditMemes()
        {
            OpenInBrowser($"{RedditUrl}/r/Memes");
        }

        public static void OpenTwitter()
        {
            OpenInBrowser(TwitterUrl);
        }
    }
}
