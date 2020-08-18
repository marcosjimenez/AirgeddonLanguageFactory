#!/usr/bin/env bash
#Title........: language_strings.sh
#Description..: All the translated strings that airgeddon uses are located here.
#Author.......: v1s1t0r
#Bash Version.: 4.2 or later

#Set language_strings file version
#shellcheck disable=SC2034
function set_language_strings_version() {

	debug_print

	language_strings_version="¬Data.Version¬"
}

#Set different language text strings
#shellcheck disable=SC2154
#shellcheck disable=SC2034
function initialize_language_strings() {

	debug_print

	if [[ "$(declare -p wps_data_array 2> /dev/null)" != "declare -A"* ]]; then
		declare -gA wps_data_array
	fi

	if [[ "$(declare -p interfaces_band_info 2> /dev/null)" != "declare -A"* ]]; then
		declare -gA interfaces_band_info
	fi

	if [[ "$(declare -p function_hooks 2> /dev/null)" != "declare -A"* ]]; then
		declare -gA function_hooks
	fi

	declare -A unknown_chipset
	¬Data.Translations.unknown_chipset : { translation | unknown_chipset["¬translation.Language¬"]="¬translation.Text¬"	
	}¬
	unknown_chipsetvar="${unknown_chipset[${language}]}"

	declare -A hintprefix
	¬Data.Translations.hintprefix : { translation | hintprefix["¬translation.Language¬"]="¬translation.Text¬" 
	}¬
	hintvar="*${hintprefix[${language}]}*"
	escaped_hintvar="\*${hintprefix[${language}]}\*"

	declare -A optionaltool_needed
	¬Data.Translations.optionaltool_needed : { translation | optionaltool_needed["¬translation.Language¬"]="¬translation.Text¬" 
	}¬
	declare -A under_construction
	¬Data.Translations.under_construction : { translation | under_construction["¬translation.Language¬"]="¬translation.Text¬" 
	}¬	under_constructionvar="${under_construction[${language}]}"

	declare -gA possible_package_names_text
	¬Data.Translations.possible_package_names_text : { translation | possible_package_names_text["¬translation.Language¬"]="¬translation.Text¬"
	}¬
	declare -gA disabled_text
	¬Data.Translations.disabled_text : { translation | disabled_text["¬translation.Language¬"]="¬translation.Text¬"
	}¬
	declare -gA reboot_required
	¬Data.Translations.reboot_required : { translation | reboot_required["¬translation.Language¬"]="¬translation.Text¬"
	}¬
	declare -gA docker_image
	¬Data.Translations.docker_image : { translation | docker_image["¬translation.Language¬"]="¬translation.Text¬"
	}¬
	declare -gA et_misc_texts
	¬Data.Translations.et_misc_texts : { translation | et_misc_texts["¬translation.Language¬", ¬translation.Index¬]="¬translation.Text¬"
	}¬
	declare -gA wps_texts
	¬Data.Translations.wps_texts : { translation | wps_texts["¬translation.Language¬", ¬translation.Index¬]="¬translation.Text¬"
	}¬
	declare -gA wep_texts
	¬Data.Translations.wep_texts : { translation | wep_texts["¬translation.Language¬", ¬translation.Index¬]="¬translation.Text¬"
	}¬
	declare -gA asleap_texts
	¬Data.Translations.asleap_texts : { translation | asleap_texts["¬translation.Language¬", ¬translation.Index¬]="¬translation.Text¬"
	}¬
	declare -gA jtr_texts
	¬Data.Translations.jtr_texts : { translation | jtr_texts["¬translation.Language¬", ¬translation.Index¬]="¬translation.Text¬"
	}¬
	declare -gA hashcat_texts
	¬Data.Translations.hashcat_texts : { translation | hashcat_texts["¬translation.Language¬", ¬translation.Index¬]="¬translation.Text¬"
	}¬
	declare -gA aircrack_texts
	¬Data.Translations.aircrack_texts : { translation | aircrack_texts["¬translation.Language¬", ¬translation.Index¬]="¬translation.Text¬"
	}¬
	declare -gA enterprise_texts
	¬Data.Translations.enterprise_texts : { translation | enterprise_texts["¬translation.Language¬", ¬translation.Index¬]="¬translation.Text¬"
	}¬
	declare -gA footer_texts
	¬Data.Translations.footer_texts : { translation | footer_texts["¬translation.Language¬", ¬translation.Index¬]="¬translation.Text¬"
	}¬
	declare -gA arr
	¬Data.Translations.arr: { translation | arr["¬translation.Language¬", ¬translation.Index¬]="¬translation.Text¬"
	}¬
}

#Expand escaped variables in language strings with their actual values
function replace_string_vars() {

	debug_print

	local message
	local replace
	message=${arr[${1},${2}]}
	parsed_message=$(echo "${message}" | sed -E 's/\"/\\\"/g')
	eval "echo -e \"${parsed_message}\""
}

#Print a language string
#shellcheck disable=SC2154
function language_strings() {

	debug_print

	local message
	message=$(replace_string_vars "${@}")

	case "${3}" in
		"yellow")
			interrupt_checkpoint "${2}" "${3}"
			echo_yellow "${message}"
		;;
		"blue")
			echo_blue "${message}"
		;;
		"red")
			echo_red "${message}"
		;;
		"green")
			if [[ "${2}" -ne "${abort_question}" ]] 2>/dev/null && [[ "${2}" != "${abort_question}" ]]; then
				interrupt_checkpoint "${2}" "${3}"
			fi
			echo_green "${message}"
		;;
		"pink")
			echo_pink "${message}"
		;;
		"white")
			echo_white "${message}"
		;;
		"title")
			generate_dynamic_line "${message}" "title"
		;;
		"read")
			interrupt_checkpoint "${2}" "${3}"
			read -p "${message}" -r
		;;
		"multiline")
			echo -ne "${message}"
		;;
		"hint")
			echo_brown "${hintvar} ${pink_color}${message}"
		;;
		"separator")
			generate_dynamic_line "${message}" "separator"
		;;
		"warning")
			echo_yellow "${message}"
		;;
		"under_construction")
			echo_red_slim "${message} (${under_constructionvar})"
		;;
		*)
			if [ -z "${3}" ]; then
				last_echo "${message}" "${normal_color}"
			else
				special_text_missed_optional_tool "${1}" "${2}" "${3}"
			fi
		;;
	esac
}