using System;
using System.Collections.Generic;
using System.Text;

namespace Airgeddon.LanguageFactory.Models
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Estilos de nombres", Justification = "<pendiente>")]
    public class TranslationFile
    {
        public List<TranslationItem> unknown_chipset { get; set; } 
        public List<TranslationItem> hintprefix { get; set; }
        public List<TranslationItem> optionaltool_needed { get; set; }
        public List<TranslationItem> under_construction { get; set; }
        public List<TranslationItem> possible_package_names_text { get; set; }
        public List<TranslationItem> disabled_text { get; set; }
        public List<TranslationItem> reboot_required { get; set; }
        public List<TranslationItem> docker_image { get; set; }
        public List<TranslationItemWithIndex> et_misc_texts { get; set; }
        public List<TranslationItemWithIndex> wps_texts { get; set; }
        public List<TranslationItemWithIndex> asleap_texts { get; set; }
        public List<TranslationItemWithIndex> jtr_texts { get; set; }
        public List<TranslationItemWithIndex> hashcat_texts { get; set; }
        public List<TranslationItemWithIndex> aircrack_texts { get; set; }
        public List<TranslationItemWithIndex> enterprise_texts { get; set; }
        public List<TranslationItemWithIndex> footer_texts { get; set; }
        public List<TranslationItemWithIndex> arr { get; set; } 

        public TranslationFile()
        {
            unknown_chipset = new List<TranslationItem>();
            hintprefix = new List<TranslationItem>();
            optionaltool_needed = new List<TranslationItem>();
            under_construction = new List<TranslationItem>();
            possible_package_names_text = new List<TranslationItem>();
            disabled_text = new List<TranslationItem>();
            reboot_required = new List<TranslationItem>();
            docker_image = new List<TranslationItem>();
            et_misc_texts = new List<TranslationItemWithIndex>();
            wps_texts = new List<TranslationItemWithIndex>();
            asleap_texts = new List<TranslationItemWithIndex>();
            jtr_texts = new List<TranslationItemWithIndex>();
            hashcat_texts = new List<TranslationItemWithIndex>();
            aircrack_texts = new List<TranslationItemWithIndex>();
            enterprise_texts = new List<TranslationItemWithIndex>();
            footer_texts = new List<TranslationItemWithIndex>();
            arr = new List<TranslationItemWithIndex>();
        }
    }
}
