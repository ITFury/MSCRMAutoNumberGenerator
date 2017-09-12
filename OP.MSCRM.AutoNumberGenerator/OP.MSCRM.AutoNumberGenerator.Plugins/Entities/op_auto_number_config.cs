using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace OP.MSCRM.AutoNumberGenerator.Plugins.Entities
{
    /// <summary>
    /// Entity op_auto_number_config members
    /// </summary>
    [EntityLogicalName("op_auto_number_config")]
    public partial class op_auto_number_config : Entity
    {

        /// <summary>
        /// Entity Name where display Auto Number
        /// </summary>
        public string op_entity_name
        {
            get
            {
                return GetAttributeValue<string>(op_entity_nameAttribute);
            }
            set
            {
                this[op_entity_nameAttribute] = value;
            }
        }


        /// <summary>
        /// Field Name where display Auto Number
        /// </summary>
        public string op_field_name
        {
            get
            {
                return GetAttributeValue<string>(op_field_nameAttribute);
            }
            set
            {
                this[op_field_nameAttribute] = value;
            }
        }


        /// <summary>
        /// Auto Number Format
        /// </summary>
        public string op_format
        {
            get
            {
                return GetAttributeValue<string>(op_formatAttribute);
            }
            set
            {
                this[op_formatAttribute] = value;
            }
        }


        /// <summary>
        /// Auto Number Format number length
        /// </summary>
        public int? op_format_number_length
        {
            get
            {
                return GetAttributeValue<int?>(op_format_number_lengthAttribute);
            }
            set
            {
                this[op_format_number_lengthAttribute] = value;
            }
        }


        /// <summary>
        /// Override Auto Number sequence
        /// </summary>
        public bool op_override_sequence
        {
            get
            {
                return GetAttributeValue<bool>(op_override_sequenceAttribute);
            }
            set
            {
                this[op_override_sequenceAttribute] = value;
            }
        }


        /// <summary>
        /// Auto Number current sequence
        /// </summary>
        public int? op_sequence
        {
            get
            {
                return GetAttributeValue<int?>(op_sequenceAttribute);
            }
            set
            {
                this[op_sequenceAttribute] = value;
            }
        }
    }
}
