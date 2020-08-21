namespace Airgeddon.LanguageFactory.Models
{
    using System.Collections.Generic;
    using System.Linq;

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
        public List<TranslationItemWithIndex> wep_texts { get; set; }
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

        public void SortIndexItems()
        {

            aircrack_texts = aircrack_texts.OrderBy(x => int.Parse(x.Index)).ToList();
            arr = arr.OrderBy(x => int.Parse(x.Index)).ToList();
            asleap_texts = asleap_texts.OrderBy(x => int.Parse(x.Index)).ToList();
            et_misc_texts = et_misc_texts.OrderBy(x => int.Parse(x.Index)).ToList();
            enterprise_texts = enterprise_texts.OrderBy(x => int.Parse(x.Index)).ToList();
            footer_texts = footer_texts.OrderBy(x => int.Parse(x.Index)).ToList();
            hashcat_texts = hashcat_texts.OrderBy(x => int.Parse(x.Index)).ToList();
            jtr_texts = jtr_texts.OrderBy(x => int.Parse(x.Index)).ToList();
            wps_texts = wps_texts.OrderBy(x => int.Parse(x.Index)).ToList();
        }

    }
}
