using System;


namespace OP.MSCRM.AutoNumberGenerator.Plugins.ExceptionHandling
{
    /// <summary>
    /// Plugin execution exception handling
    /// </summary>
    public class PluginException : Exception
    {
        public PluginException(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Exception name
        /// </summary>
        public string  Name { get; private set; }

        /// <summary>
        /// Exception description
        /// </summary>
        public string Description { get; private set; }
    }
}
