using System;

namespace Salud.Security.SSO.Classes
{
    /// <summary>
    /// Define un mensaje para utilizar en formato JSON
    /// </summary>
    [Serializable]
    public class SSOMessage
    {
        public int id { get; set; }
        public string message { get; set; }
        public int type { get; set; }
        public DateTime date { get; set; }
    }
}