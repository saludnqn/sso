using System.Collections.Generic;

namespace Salud.Security.SSO
{
    [System.Serializable]
    public class SSOMenuItem
    {
        public int id { get; set; }
        public string text { get; set; }
        public string url { get; set; }
        public string fullUrl { get; set; }
        public bool isSeparator { get; set; }
        public string imageURL { get; set; }
        public int priority { get; set; }
        public List<SSOMenuItem> items = new List<SSOMenuItem>();
    }
}