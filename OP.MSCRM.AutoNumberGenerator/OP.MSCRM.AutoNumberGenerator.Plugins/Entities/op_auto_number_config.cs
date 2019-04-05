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
        /// Entity Name where display Auto-Number
        /// </summary>
        [AttributeLogicalName(op_entity_nameAttribute)]
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
        /// Field Name where display Auto-Number
        /// </summary>
        [AttributeLogicalName(op_field_nameAttribute)]
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
        /// Auto-Number Start Number
        /// </summary>
        [AttributeLogicalName(op_start_numberAttribute)]
        public int? op_start_number
        {
            get
            {
                return GetAttributeValue<int?>(op_start_numberAttribute);
            }
            set
            {
                this[op_start_numberAttribute] = value;
            }
        }


        /// <summary>
        /// Auto-Number Step (Increment)
        /// </summary>
        [AttributeLogicalName(op_number_stepAttribute)]
        public int? op_number_step
        {
            get
            {
                return GetAttributeValue<int?>(op_number_stepAttribute);
            }
            set
            {
                this[op_number_stepAttribute] = value;
            }
        }


        /// <summary>
        /// Auto-Number length
        /// </summary>
        [AttributeLogicalName(op_number_lengthAttribute)]
        public int? op_number_length
        {
            get
            {
                return GetAttributeValue<int?>(op_number_lengthAttribute);
            }
            set
            {
                this[op_number_lengthAttribute] = value;
            }
        }


        /// <summary>
        /// Rearrange Auto-Number sequence after record delete
        /// </summary>
        [AttributeLogicalName(op_rearrange_sequenceAttribute)]
        public bool op_rearrange_sequence
        {
            get
            {
                return GetAttributeValue<bool>(op_rearrange_sequenceAttribute);
            }
            set
            {
                this[op_rearrange_sequenceAttribute] = value;
            }
        }


        /// <summary>
        /// Auto-Number current sequence
        /// </summary>
        [AttributeLogicalName(op_sequenceAttribute)]
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

        /// <summary>
        /// Auto-Number Prefix
        /// </summary>
        [AttributeLogicalName(op_number_prefixAttribute)]
        public string op_number_prefix
        {
            get
            {
                return GetAttributeValue<string>(op_number_prefixAttribute);
            }
            set
            {
                this[op_number_prefixAttribute] = value;
            }
        }

        /// <summary>
        /// Auto-Number Suffix
        /// </summary>
        [AttributeLogicalName(op_number_suffixAttribute)]
        public string op_number_suffix
        {
            get
            {
                return GetAttributeValue<string>(op_number_suffixAttribute);
            }
            set
            {
                this[op_number_suffixAttribute] = value;
            }
        }

        /// <summary>
        /// Auto-Number Preview
        /// </summary>
        [AttributeLogicalName(op_number_previewAttribute)]
        public string op_number_preview
        {
            get
            {
                return GetAttributeValue<string>(op_number_previewAttribute);
            }
            set
            {
                this[op_number_previewAttribute] = value;
            }
        }

        /// <summary>
        /// Lock Auto-Number
        /// </summary>
        [AttributeLogicalName(op_is_lockedAttribute)]
        public bool op_is_locked
        {
            get
            {
                return GetAttributeValue<bool>(op_is_lockedAttribute);
            }
            set
            {
                this[op_is_lockedAttribute] = value;
            }
        }
    }
}
