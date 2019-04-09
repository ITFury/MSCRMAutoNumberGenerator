using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace OP.MSCRM.AutoNumberGenerator.Plugins.Entities
{
    /// <summary>
    /// Entity op_autonumberconfig members
    /// </summary>
    [EntityLogicalName("op_autonumberconfig")]
    public partial class Op_AutoNumberConfig : Entity
    {

        /// <summary>
        /// Entity Schema Name where display Auto-Number
        /// </summary>
        [AttributeLogicalName(EntitySchemaNameAttribute)]
        public string EntitySchemaName
        {
            get
            {
                return GetAttributeValue<string>(EntitySchemaNameAttribute);
            }
            set
            {
                this[EntitySchemaNameAttribute] = value;
            }
        }


        /// <summary>
        /// Field Schema Name where display Auto-Number
        /// </summary>
        [AttributeLogicalName(FieldSchemaNameAttribute)]
        public string FieldSchemaName
        {
            get
            {
                return GetAttributeValue<string>(FieldSchemaNameAttribute);
            }
            set
            {
                this[FieldSchemaNameAttribute] = value;
            }
        }


        /// <summary>
        /// Auto-Number Start Number
        /// </summary>
        [AttributeLogicalName(StartAttribute)]
        public int? Start
        {
            get
            {
                return GetAttributeValue<int?>(StartAttribute);
            }
            set
            {
                this[StartAttribute] = value;
            }
        }


        /// <summary>
        /// Auto-Number Increment
        /// </summary>
        [AttributeLogicalName(IncrementAttribute)]
        public int? Increment
        {
            get
            {
                return GetAttributeValue<int?>(IncrementAttribute);
            }
            set
            {
                this[IncrementAttribute] = value;
            }
        }


        /// <summary>
        /// Auto-Number length
        /// </summary>
        [AttributeLogicalName(LengthAttribute)]
        public int? Length
        {
            get
            {
                return GetAttributeValue<int?>(LengthAttribute);
            }
            set
            {
                this[LengthAttribute] = value;
            }
        }


        /// <summary>
        /// Rearrange Auto-Number sequence after record delete
        /// </summary>
        [AttributeLogicalName(IsRearrangeSequenceAttribute)]
        public bool IsRearrangeSequence
        {
            get
            {
                return GetAttributeValue<bool>(IsRearrangeSequenceAttribute);
            }
            set
            {
                this[IsRearrangeSequenceAttribute] = value;
            }
        }


        /// <summary>
        /// Auto-Number current sequence
        /// </summary>
        [AttributeLogicalName(SequenceAttribute)]
        public int? Sequence
        {
            get
            {
                return GetAttributeValue<int?>(SequenceAttribute);
            }
            set
            {
                this[SequenceAttribute] = value;
            }
        }

        /// <summary>
        /// Auto-Number Prefix
        /// </summary>
        [AttributeLogicalName(PrefixAttribute)]
        public string Prefix
        {
            get
            {
                return GetAttributeValue<string>(PrefixAttribute);
            }
            set
            {
                this[PrefixAttribute] = value;
            }
        }

        /// <summary>
        /// Auto-Number Suffix
        /// </summary>
        [AttributeLogicalName(SuffixAttribute)]
        public string Suffix
        {
            get
            {
                return GetAttributeValue<string>(SuffixAttribute);
            }
            set
            {
                this[SuffixAttribute] = value;
            }
        }

        /// <summary>
        /// Auto-Number Preview
        /// </summary>
        [AttributeLogicalName(PreviewAttribute)]
        public string Preview
        {
            get
            {
                return GetAttributeValue<string>(PreviewAttribute);
            }
            set
            {
                this[PreviewAttribute] = value;
            }
        }

        /// <summary>
        /// Lock Auto-Number
        /// </summary>
        [AttributeLogicalName(IsLockedAttribute)]
        public bool IsLocked
        {
            get
            {
                return GetAttributeValue<bool>(IsLockedAttribute);
            }
            set
            {
                this[IsLockedAttribute] = value;
            }
        }
    }
}
