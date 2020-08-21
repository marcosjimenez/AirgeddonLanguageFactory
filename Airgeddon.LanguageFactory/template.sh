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
	¬Data.unknown_chipset¬
	unknown_chipsetvar="${unknown_chipset[${language}]}"

	declare -A hintprefix
	¬Data.hintprefix¬
	hintvar="*${hintprefix[${language}]}*"
	escaped_hintvar="\*${hintprefix[${language}]}\*"

	declare -A optionaltool_needed
	¬Data.optionaltool_needed¬
	declare -A under_construction
	¬Data.under_construction¬	under_constructionvar="${under_construction[${language}]}"

	declare -gA possible_package_names_text
	¬Data.possible_package_names_text¬
	declare -gA disabled_text
	¬Data.disabled_text¬
	declare -gA reboot_required
	¬Data.reboot_required¬
	declare -gA docker_image
	¬Data.docker_image¬
	declare -gA et_misc_texts
	¬Data.et_misc_texts¬
	declare -gA wps_texts
	¬Data.wps_texts¬
	declare -gA wep_texts
	¬Data.wep_texts¬
	declare -gA asleap_texts
	¬Data.asleap_texts¬
	declare -gA jtr_texts
	¬Data.jtr_texts¬
	declare -gA hashcat_texts
	¬Data.hashcat_texts¬
	declare -gA aircrack_texts
	¬Data.aircrack_texts¬
	declare -gA enterprise_texts
	¬Data.enterprise_texts¬
	declare -gA footer_texts
	¬Data.footer_texts¬
	declare -gA arr
	¬Data.arr¬
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