﻿#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;

namespace MediaInfo
{
  /// <summary>
  /// Describes methods to manipulate language data
  /// </summary>
  public static class LanguageHelper
  {
    private const string UnknownLanguage = "Unknown";

    #region Dictionary LCID - langage name

    private static readonly Dictionary<int, string> LanguageLcids = new Dictionary<int, string>
    {
      { 0x0401, "Arabic" },
      { 0x0402, "Bulgarian" },
      { 0x0403, "Catalan" },
      { 0x0404, "Chinese" },
      { 0x0405, "Czech" },
      { 0x0406, "Danish" },
      { 0x0407, "German" },
      { 0x0408, "Greek" },
      { 0x0409, "English" },
      { 0x040a, "Spanish" },
      { 0x040b, "Finnish" },
      { 0x040c, "French" },
      { 0x040d, "Hebrew" },
      { 0x040e, "Hungarian" },
      { 0x040f, "Icelandic" },
      { 0x0410, "Italian" },
      { 0x0411, "Japanese" },
      { 0x0412, "Korean" },
      { 0x0413, "Dutch" },
      { 0x0414, "Norwegian" },
      { 0x0415, "Polish" },
      { 0x0416, "Portuguese" },
      { 0x0417, "Romansh" },
      { 0x0418, "Romanian" },
      { 0x0419, "Russian" },
      { 0x041a, "Croatian" },
      { 0x041b, "Slovak" },
      { 0x041c, "Albanian" },
      { 0x041d, "Swedish" },
      { 0x041e, "Thai" },
      { 0x041f, "Turkish" },
      { 0x0420, "Urdu" },
      { 0x0421, "Indonesian" },
      { 0x0422, "Ukrainian" },
      { 0x0423, "Belarusian" },
      { 0x0424, "Slovenian" },
      { 0x0425, "Estonian" },
      { 0x0426, "Latvian" },
      { 0x0427, "Lithuanian" },
      { 0x0428, "Tajik" },
      { 0x0429, "Persian" },
      { 0x042a, "Vietnamese" },
      { 0x042b, "Armenian" },
      { 0x042c, "Azeri" },
      { 0x042d, "Basque" },
      { 0x042e, "Upper Sorbian" },
      { 0x042f, "Macedonian" },
      { 0x0432, "Setswana" },
      { 0x0434, "isiXhosa" },
      { 0x0435, "isiZulu" },
      { 0x0436, "Afrikaans" },
      { 0x0437, "Georgian" },
      { 0x0438, "Faroese" },
      { 0x0439, "Hindi" },
      { 0x043a, "Maltese" },
      { 0x043b, "Sami" },
      { 0x043c, "Irish" },
      { 0x043e, "Malay" },
      { 0x043f, "Kazakh" },
      { 0x0440, "Kyrgyz" },
      { 0x0441, "Kiswahili" },
      { 0x0442, "Turkmen" },
      { 0x0443, "Uzbek" },
      { 0x0444, "Tatar" },
      { 0x0445, "Bengali" },
      { 0x0446, "Punjabi" },
      { 0x0447, "Gujarati" },
      { 0x0448, "Oriya" },
      { 0x0449, "Tamil" },
      { 0x044a, "Telugu" },
      { 0x044b, "Kannada" },
      { 0x044c, "Malayalam" },
      { 0x044d, "Assamese" },
      { 0x044e, "Marathi" },
      { 0x044f, "Sanskrit" },
      { 0x0450, "Mongolian" },
      { 0x0451, "Tibetan" },
      { 0x0452, "Welsh" },
      { 0x0453, "Khmer" },
      { 0x0454, "Lao" },
      { 0x0456, "Galician" },
      { 0x0457, "Konkani" },
      { 0x045a, "Syriac" },
      { 0x045b, "Sinhala" },
      { 0x045d, "Inuktitut" },
      { 0x045e, "Amharic" },
      { 0x045f, "Tamazight" },
      { 0x0461, "Nepali" },
      { 0x0462, "Frisian" },
      { 0x0463, "Pashto" },
      { 0x0464, "Filipino" },
      { 0x0465, "Divehi" },
      { 0x0468, "Hausa" },
      { 0x046a, "Yoruba" },
      { 0x046b, "Quechua" },
      { 0x046c, "Sesotho sa Leboa" },
      { 0x046d, "Bashkir" },
      { 0x046e, "Luxembourgish" },
      { 0x046f, "Greenlandic" },
      { 0x0470, "Igbo" },
      { 0x0478, "Yi" },
      { 0x047a, "Mapudungun" },
      { 0x047c, "Mohawk" },
      { 0x047e, "Breton" },
      { 0x0480, "Uyghur" },
      { 0x0481, "Maori" },
      { 0x0482, "Occitan" },
      { 0x0483, "Corsican" },
      { 0x0484, "Alsatian" },
      { 0x0485, "Yakut" },
      { 0x0486, "K'iche" },
      { 0x0487, "Kinyarwanda" },
      { 0x0488, "Wolof" },
      { 0x048c, "Dari" },
      { 0x0491, "Scottish Gaelic" },
    };

    #endregion

    #region Dictionary ISO-639 - LCID

    private static readonly Dictionary<string, int> Lcids = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
      { "ARA", 0x0401 },
      { "AR", 0x0401 },
      { "BUL", 0x0402 },
      { "BG", 0x0402 },
      { "CAT", 0x0403 },
      { "CA", 0x0403 },
      { "ZHO", 0x0404 },
      { "ZH", 0x0404 },
      { "CES", 0x0405 },
      { "CS", 0x0405 },
      { "DAN", 0x0406 },
      { "DA", 0x0406 },
      { "DEU", 0x0407 },
      { "DE", 0x0407 },
      { "ELL", 0x0408 },
      { "EL", 0x0408 },
      { "ENG", 0x0409 },
      { "EN", 0x0409 },
      { "SPA", 0x040a },
      { "ES", 0x040a },
      { "FIN", 0x040b },
      { "FI", 0x040b },
      { "FRA", 0x040c },
      { "FR", 0x040c },
      { "HEB", 0x040d },
      { "HE", 0x040d },
      { "HUN", 0x040e },
      { "HU", 0x040e },
      { "ISL", 0x040f },
      { "IS", 0x040f },
      { "ITA", 0x0410 },
      { "IT", 0x0410 },
      { "JPN", 0x0411 },
      { "JA", 0x0411 },
      { "KOR", 0x0412 },
      { "KO", 0x0412 },
      { "NLD", 0x0413 },
      { "NL", 0x0413 },
      { "NOB", 0x0414 },
      { "NB", 0x0414 },
      { "POL", 0x0415 },
      { "PL", 0x0415 },
      { "POR", 0x0416 },
      { "PT", 0x0416 },
      { "ROH", 0x0417 },
      { "RM", 0x0417 },
      { "RON", 0x0418 },
      { "RO", 0x0418 },
      { "RUS", 0x0419 },
      { "RU", 0x0419 },
      { "HRV", 0x041a },
      { "HR", 0x041a },
      { "SLK", 0x041b },
      { "SK", 0x041b },
      { "SQI", 0x041c },
      { "SQ", 0x041c },
      { "SWE", 0x041d },
      { "SV", 0x041d },
      { "THA", 0x041e },
      { "TH", 0x041e },
      { "TUR", 0x041f },
      { "TR", 0x041f },
      { "URD", 0x0420 },
      { "UR", 0x0420 },
      { "IND", 0x0421 },
      { "ID", 0x0421 },
      { "UKR", 0x0422 },
      { "UK", 0x0422 },
      { "BEL", 0x0423 },
      { "BE", 0x0423 },
      { "SLV", 0x0424 },
      { "SL", 0x0424 },
      { "EST", 0x0425 },
      { "ET", 0x0425 },
      { "LAV", 0x0426 },
      { "LV", 0x0426 },
      { "LIT", 0x0427 },
      { "LT", 0x0427 },
      { "TGK", 0x0428 },
      { "TG", 0x0428 },
      { "FAS", 0x0429 },
      { "FA", 0x0429 },
      { "VIE", 0x042a },
      { "VI", 0x042a },
      { "HYE", 0x042b },
      { "HY", 0x042b },
      { "AZE", 0x042c },
      { "AZ", 0x042c },
      { "EUS", 0x042d },
      { "EU", 0x042d },
      { "HSB", 0x042e },
      { "MKD", 0x042f },
      { "MK", 0x042f },
      { "TSN", 0x0432 },
      { "TN", 0x0432 },
      { "XHO", 0x0434 },
      { "XH", 0x0434 },
      { "ZUL", 0x0435 },
      { "ZU", 0x0435 },
      { "AFR", 0x0436 },
      { "AF", 0x0436 },
      { "KAT", 0x0437 },
      { "KA", 0x0437 },
      { "FAO", 0x0438 },
      { "FO", 0x0438 },
      { "HIN", 0x0439 },
      { "HI", 0x0439 },
      { "MLT", 0x043a },
      { "MT", 0x043a },
      { "SME", 0x043b },
      { "SE", 0x043b },
      { "GLE", 0x043c },
      { "GA", 0x043c },
      { "MSA", 0x043e },
      { "MS", 0x043e },
      { "KAZ", 0x043f },
      { "KK", 0x043f },
      { "KIR", 0x0440 },
      { "KY", 0x0440 },
      { "SWA", 0x0441 },
      { "SW", 0x0441 },
      { "TUK", 0x0442 },
      { "TK", 0x0442 },
      { "UZB", 0x0443 },
      { "UZ", 0x0443 },
      { "TAT", 0x0444 },
      { "TT", 0x0444 },
      { "BNG", 0x0445 },
      { "BN", 0x0445 },
      { "PAN", 0x0446 },
      { "PA", 0x0446 },
      { "GUJ", 0x0447 },
      { "GU", 0x0447 },
      { "ORI", 0x0448 },
      { "OR", 0x0448 },
      { "TAM", 0x0449 },
      { "TA", 0x0449 },
      { "TEL", 0x044a },
      { "TE", 0x044a },
      { "KAN", 0x044b },
      { "KN", 0x044b },
      { "MYM", 0x044c },
      { "ML", 0x044c },
      { "ASM", 0x044d },
      { "AS", 0x044d },
      { "MAR", 0x044e },
      { "MR", 0x044e },
      { "SAN", 0x044f },
      { "SA", 0x044f },
      { "MON", 0x0450 },
      { "MN", 0x0450 },
      { "BOD", 0x0451 },
      { "BO", 0x0451 },
      { "CYM", 0x0452 },
      { "CY", 0x0452 },
      { "KHM", 0x0453 },
      { "KM", 0x0453 },
      { "LAO", 0x0454 },
      { "LO", 0x0454 },
      { "GLG", 0x0456 },
      { "GL", 0x0456 },
      { "KOK", 0x0457 },
      { "SYR", 0x045a },
      { "SIN", 0x045b },
      { "SI", 0x045b },
      { "IKU", 0x045d },
      { "IU", 0x045d },
      { "AMH", 0x045e },
      { "AM", 0x045e },
      { "TZM", 0x045f },
      { "NEP", 0x0461 },
      { "NE", 0x0461 },
      { "FRY", 0x0462 },
      { "FY", 0x0462 },
      { "PUS", 0x0463 },
      { "PS", 0x0463 },
      { "FIL", 0x0464 },
      { "DIV", 0x0465 },
      { "DV", 0x0465 },
      { "HAU", 0x0468 },
      { "HA", 0x0468 },
      { "YOR", 0x046a },
      { "YO", 0x046a },
      { "QUB", 0x046b },
      { "QUZ", 0x046b },
      { "NSO", 0x046c },
      { "BAK", 0x046d },
      { "BA", 0x046d },
      { "LTZ", 0x046e },
      { "LB", 0x046e },
      { "KAL", 0x046f },
      { "KL", 0x046f },
      { "IBO", 0x0470 },
      { "IG", 0x0470 },
      { "III", 0x0478 },
      { "II", 0x0478 },
      { "ARN", 0x047a },
      { "MOH", 0x047c },
      { "BRE", 0x047e },
      { "BR", 0x047e },
      { "IVL", 0x047f },
      { "IV", 0x047f },
      { "UIG", 0x0480 },
      { "UG", 0x0480 },
      { "MRI", 0x0481 },
      { "MI", 0x0481 },
      { "OCI", 0x0482 },
      { "OC", 0x0482 },
      { "COS", 0x0483 },
      { "CO", 0x0483 },
      { "GSW", 0x0484 },
      { "SAH", 0x0485 },
      { "QUT", 0x0486 },
      { "KIN", 0x0487 },
      { "RW", 0x0487 },
      { "WOL", 0x0488 },
      { "WO", 0x0488 },
      { "PRS", 0x048c },
      { "GLA", 0x0491 },
      { "GD", 0x0491 },
      { "NNO", 0x0c14 },
      { "NN", 0x0c14 },
      { "SRP", 0x0c1a },
      { "SR", 0x0c1a },
      { "DSB", 0x0c2e },
      { "SMF", 0x0c3b },
      { "QUE", 0x0c6b },
      { "SMG", 0x103b },
      { "QUP", 0x106b },
      { "HRB", 0x141a },
      { "SMJ", 0x143b },
      { "BSB", 0x181a },
      { "BS", 0x181a },
      { "SMK", 0x183b },
      { "SRS", 0x1c1a },
      { "SMA", 0x1c3b },
      { "SRN", 0x201a },
      { "SMB", 0x203b },
      { "BSC", 0x241a },
      { "SMS", 0x243b },
      { "SMN", 0x283b },
    };

    #endregion

    #region Dictionary ISO-639 - language name

    private static readonly Dictionary<string, string> Languages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
      { "ABK", "Abkhazian" },
      { "AB", "Abkhazian" },
      { "ACE", "Achinese" },
      { "ACH", "Acoli" },
      { "ADA", "Adangme" },
      { "AAR", "Afar" },
      { "AA", "Afar" },
      { "AFH", "Afrihili" },
      { "AFR", "Afrikaans" },
      { "AF", "Afrikaans" },
      { "AKA", "Akan" },
      { "AK", "Akan" },
      { "AKK", "Akkadian" },
      { "ALB", "Albanian" },
      { "SQI", "Albanian" },
      { "SQ", "Albanian" },
      { "ALE", "Aleut" },
      { "AMH", "Amharic" },
      { "AM", "Amharic" },
      { "ARA", "Arabic" },
      { "AR", "Arabic" },
      { "ARG", "Aragonese" },
      { "AN", "Aragonese" },
      { "ARC", "Aramaic" },
      { "ARP", "Arapaho" },
      { "ARN", "Araucanian" },
      { "ARW", "Arawak" },
      { "ARM", "Armenian" },
      { "HY", "Armenian" },
      { "HYE", "Armenian" },
      { "AS", "Assamese" },
      { "ASM", "Assamese" },
      { "AVA", "Avaric" },
      { "AV", "Avaric" },
      { "AVE", "Avestan" },
      { "AE", "Avestan" },
      { "AWA", "Awadhi" },
      { "AYM", "Aymara" },
      { "AY", "Aymara" },
      { "AZE", "Azerbaijani" },
      { "AZ", "Azerbaijani" },
      { "BAN", "Balinese" },
      { "BAL", "Baluchi" },
      { "BAM", "Bambara" },
      { "BM", "Bambara" },
      { "BAD", "Banda" },
      { "BAS", "Basa" },
      { "BAK", "Bashkir" },
      { "BA", "Bashkir" },
      { "BAQ", "Basque" },
      { "EU", "Basque" },
      { "EUS", "Basque" },
      { "BEJ", "Beja" },
      { "BEL", "Belarusian" },
      { "BE", "Belarusian" },
      { "BEM", "Bemba" },
      { "BEN", "Bengali" },
      { "BN", "Bengali" },
      { "BHO", "Bhojpuri" },
      { "BH", "Bihari" },
      { "BIH", "Bihari" },
      { "BIK", "Bikol" },
      { "BIN", "Bini" },
      { "BIS", "Bislama" },
      { "BI", "Bislama" },
      { "NOB", "Norwegian Bokmal" },
      { "NB", "Norwegian Bokmal" },
      { "BOS", "Bosnian" },
      { "BS", "Bosnian" },
      { "BRA", "Braj" },
      { "BRE", "Breton" },
      { "BR", "Breton" },
      { "BUG", "Buginese" },
      { "BUL", "Bulgarian" },
      { "BG", "Bulgarian" },
      { "BUA", "Buriat" },
      { "BUR", "Burmese" },
      { "MY", "Burmese" },
      { "MYA", "Burmese" },
      { "CAD", "Caddo" },
      { "CAR", "Carib" },
      { "SPA", "Spanish" },
      { "ES", "Spanish" },
      { "ESP", "Spanish" },
      { "CAT", "Catalan" },
      { "CA", "Catalan" },
      { "CEB", "Cebuano" },
      { "CEL", "Celtic" },
      { "CHG", "Chagatai" },
      { "CH", "Chamorro" },
      { "CHA", "Chamorro" },
      { "CE", "Chechen" },
      { "CHE", "Chechen" },
      { "CHR", "Cherokee" },
      { "CHY", "Cheyenne" },
      { "CHB", "Chibcha" },
      { "CHI", "Chinese" },
      { "ZH", "Chinese" },
      { "ZH-CN", "Chinese" },
      { "ZH-TW", "Chinese" },
      { "ZHO", "Chinese" },
      { "CHP", "Chipewyan" },
      { "CHO", "Choctaw" },
      { "CHK", "Chuukese" },
      { "CHV", "Chuvash" },
      { "CV", "Chuvash" },
      { "COP", "Coptic" },
      { "COR", "Cornish" },
      { "KW", "Cornish" },
      { "CO", "Corsican" },
      { "COS", "Corsican" },
      { "CRE", "Cree" },
      { "CR", "Cree" },
      { "MUS", "Creek" },
      { "CRP", "Creoles and pidgins" },
      { "CPE", "Creoles and pidgins" },
      { "CPF", "Creoles and pidgins" },
      { "CPP", "Creoles and pidgins" },
      { "SCR", "Croatian" },
      { "HR", "Croatian" },
      { "HRV", "Croatian" },
      { "CZE", "Czech" },
      { "CS", "Czech" },
      { "CES", "Czech" },
      { "DAK", "Dakota" },
      { "DAN", "Danish" },
      { "DA", "Danish" },
      { "DAR", "Dargwa" },
      { "DAY", "Dayak" },
      { "DEL", "Delaware" },
      { "DIN", "Dinka" },
      { "DIV", "Divehi" },
      { "DV", "Divehi" },
      { "DOI", "Dogri" },
      { "DGR", "Dogrib" },
      { "DRA", "Dravidian" },
      { "DUA", "Duala" },
      { "DUT", "Dutch" },
      { "NL", "Dutch" },
      { "NLD", "Dutch" },
      { "DUM", "Dutch" },
      { "DYU", "Dyula" },
      { "DZ", "Dzongkha" },
      { "DZO", "Dzongkha" },
      { "EFI", "Efik" },
      { "EGY", "Egyptian" },
      { "EKA", "Ekajuk" },
      { "ELX", "Elamite" },
      { "ENG", "English" },
      { "EN", "English" },
      { "EN-GB", "English" },
      { "EN-US", "English" },
      { "ENM", "English" },
      { "ANG", "English" },
      { "EO", "Esperanto" },
      { "EPO", "Esperanto" },
      { "EST", "Estonian" },
      { "ET", "Estonian" },
      { "EE", "Ewe" },
      { "EWE", "Ewe" },
      { "EWO", "Ewondo" },
      { "FAN", "Fang" },
      { "FAT", "Fanti" },
      { "FAO", "Faroese" },
      { "FO", "Faroese" },
      { "FIJ", "Fijian" },
      { "FJ", "Fijian" },
      { "FIN", "Finnish" },
      { "FI", "Finnish" },
      { "FON", "Fon" },
      { "FRE", "French" },
      { "FR", "French" },
      { "FRA", "French" },
      { "FRM", "French" },
      { "FRO", "French" },
      { "FRY", "Frisian" },
      { "FY", "Frisian" },
      { "FUR", "Friulian" },
      { "FF", "Fulah" },
      { "FUL", "Fulah" },
      { "GAA", "Ga" },
      { "GLA", "Gaelic" },
      { "GD", "Gaelic" },
      { "GLG", "Gallegan" },
      { "GL", "Gallegan" },
      { "LUG", "Ganda" },
      { "LG", "Ganda" },
      { "GAY", "Gayo" },
      { "GBA", "Gbaya" },
      { "GEZ", "Geez" },
      { "GEO", "Georgian" },
      { "KA", "Georgian" },
      { "KAT", "Georgian" },
      { "GER", "German" },
      { "DE", "German" },
      { "DEU", "German" },
      { "NDS", "German" },
      { "GMH", "German" },
      { "GOH", "German" },
      { "GIL", "Gilbertese" },
      { "GON", "Gondi" },
      { "GOR", "Gorontalo" },
      { "GOT", "Gothic" },
      { "GRB", "Grebo" },
      { "GRC", "Ancient Greek" },
      { "GRE", "Greek" },
      { "EL", "Greek" },
      { "ELL", "Greek" },
      { "GRN", "Guarani" },
      { "GN", "Guarani" },
      { "GUJ", "Gujarati" },
      { "GU", "Gujarati" },
      { "HAI", "Haida" },
      { "HAU", "Hausa" },
      { "HA", "Hausa" },
      { "HAW", "Hawaiian" },
      { "HEB", "Hebrew" },
      { "HE", "Hebrew" },
      { "HER", "Herero" },
      { "HZ", "Herero" },
      { "HIL", "Hiligaynon" },
      { "HIM", "Himachali" },
      { "HIN", "Hindi" },
      { "HI", "Hindi" },
      { "HMO", "Hiri Motu" },
      { "HO", "Hiri Motu" },
      { "HIT", "Hittite" },
      { "HMN", "Hmong" },
      { "HUN", "Hungarian" },
      { "HU", "Hungarian" },
      { "HUP", "Hupa" },
      { "IBA", "Iban" },
      { "ICE", "Icelandic" },
      { "IS", "Icelandic" },
      { "ISL", "Icelandic" },
      { "IDO", "Ido" },
      { "IO", "Ido" },
      { "IBO", "Igbo" },
      { "IG", "Igbo" },
      { "IJO", "Ijo" },
      { "ILO", "Iloko" },
      { "SMN", "Inari Sami" },
      { "IND", "Indonesian" },
      { "ID", "Indonesian" },
      { "INH", "Ingush" },
      { "IKU", "Inuktitut" },
      { "IU", "Inuktitut" },
      { "IPK", "Inupiaq" },
      { "IK", "Inupiaq" },
      { "GLE", "Irish" },
      { "GA", "Irish" },
      { "MGA", "Irish" },
      { "SGA", "Irish" },
      { "ITA", "Italian" },
      { "IT", "Italian" },
      { "JPN", "Japanese" },
      { "JA", "Japanese" },
      { "JAV", "Javanese" },
      { "JV", "Javanese" },
      { "JRB", "Judeo-Arabic" },
      { "JPR", "Judeo-Persian" },
      { "KBD", "Kabardian" },
      { "KAB", "Kabyle" },
      { "KAC", "Kachin" },
      { "KAM", "Kamba" },
      { "KAN", "Kannada" },
      { "KN", "Kannada" },
      { "KAU", "Kanuri" },
      { "KR", "Kanuri" },
      { "KAA", "Kara-Kalpak" },
      { "KAR", "Karen" },
      { "KAS", "Kashmiri" },
      { "KS", "Kashmiri" },
      { "KAW", "Kawi" },
      { "KAZ", "Kazakh" },
      { "KK", "Kazakh" },
      { "KHA", "Khasi" },
      { "KHM", "Khmer" },
      { "KM", "Khmer" },
      { "KHO", "Khotanese" },
      { "KMB", "Kimbundu" },
      { "KIN", "Kinyarwanda" },
      { "RW", "Kinyarwanda" },
      { "KY", "Kirghiz" },
      { "KIR", "Kirghiz" },
      { "KV", "Komi" },
      { "KOM", "Komi" },
      { "KON", "Kongo" },
      { "KG", "Kongo" },
      { "KOK", "Konkani" },
      { "KOR", "Korean" },
      { "KO", "Korean" },
      { "KOS", "Kosraean" },
      { "KPE", "Kpelle" },
      { "KRO", "Kru" },
      { "KUM", "Kumyk" },
      { "KUR", "Kurdish" },
      { "KU", "Kurdish" },
      { "KRU", "Kurukh" },
      { "KUT", "Kutenai" },
      { "LAD", "Ladino" },
      { "LAH", "Lahnda" },
      { "LAM", "Lamba" },
      { "LAO", "Lao" },
      { "LO", "Lao" },
      { "LAT", "Latin" },
      { "LA", "Latin" },
      { "LAV", "Latvian" },
      { "LV", "Latvian" },
      { "LEZ", "Lezghian" },
      { "LN", "Lingala" },
      { "LIN", "Lingala" },
      { "LIT", "Lithuanian" },
      { "LT", "Lithuanian" },
      { "LOZ", "Lozi" },
      { "LUB", "Luba-Katanga" },
      { "LU", "Luba-Katanga" },
      { "LUA", "Luba-Lulua" },
      { "LUI", "Luiseno" },
      { "SMJ", "Lule Sami" },
      { "LUN", "Lunda" },
      { "LUS", "Lushai" },
      { "MAC", "Macedonian" },
      { "MK", "Macedonian" },
      { "MKD", "Macedonian" },
      { "MAD", "Madurese" },
      { "MAG", "Magahi" },
      { "MAI", "Maithili" },
      { "MAK", "Makasar" },
      { "MLG", "Malagasy" },
      { "MG", "Malagasy" },
      { "MAY", "Malay" },
      { "MS", "Malay" },
      { "MSA", "Malay" },
      { "MAL", "Malayalam" },
      { "ML", "Malayalam" },
      { "MLT", "Maltese" },
      { "MT", "Maltese" },
      { "MNC", "Manchu" },
      { "MDR", "Mandar" },
      { "MAN", "Mandingo" },
      { "MNI", "Manipuri" },
      { "GLV", "Manx" },
      { "GV", "Manx" },
      { "MAO", "Maori" },
      { "MI", "Maori" },
      { "MRI", "Maori" },
      { "MAR", "Marathi" },
      { "MR", "Marathi" },
      { "CHM", "Mari" },
      { "MAH", "Marshallese" },
      { "MH", "Marshallese" },
      { "MWR", "Marwari" },
      { "MAS", "Masai" },
      { "MEN", "Mende" },
      { "MIC", "Micmac" },
      { "MIN", "Minangkabau" },
      { "MOH", "Mohawk" },
      { "MOL", "Moldavian" },
      { "MO", "Moldavian" },
      { "LOL", "Mongo" },
      { "MON", "Mongolian" },
      { "MN", "Mongolian" },
      { "MOS", "Mossi" },
      { "NAH", "Nahuatl" },
      { "NAU", "Nauru" },
      { "NA", "Nauru" },
      { "NAV", "Navaho" },
      { "NV", "Navaho" },
      { "NDE", "Ndebele" },
      { "ND", "Ndebele" },
      { "NR", "Ndebele" },
      { "NBL", "Ndebele" },
      { "NDO", "Ndonga" },
      { "NG", "Ndonga" },
      { "NAP", "Neapolitan" },
      { "NEP", "Nepali" },
      { "NE", "Nepali" },
      { "NEW", "Newari" },
      { "NIA", "Nias" },
      { "NIU", "Niuean" },
      { "NON", "Norse" },
      { "SME", "Northern Sami" },
      { "SE", "Northern Sami" },
      { "NOR", "Norwegian" },
      { "NO", "Norwegian" },
      { "NNO", "Norwegian Nynorsk" },
      { "NN", "Norwegian Nynorsk" },
      { "NYM", "Nyamwezi" },
      { "NYA", "Nyanja" },
      { "NY", "Nyanja" },
      { "NYN", "Nyankole" },
      { "NYO", "Nyoro" },
      { "NZI", "Nzima" },
      { "OCI", "Occitan" },
      { "OC", "Occitan" },
      { "OJI", "Ojibwa" },
      { "OJ", "Ojibwa" },
      { "ORI", "Oriya" },
      { "OR", "Oriya" },
      { "ORM", "Oromo" },
      { "OM", "Oromo" },
      { "OSA", "Osage" },
      { "OS", "Ossetian" },
      { "OSS", "Ossetian" },
      { "PAL", "Pahlavi" },
      { "PAU", "Palauan" },
      { "PLI", "Pali" },
      { "PI", "Pali" },
      { "PAM", "Pampanga" },
      { "PAG", "Pangasinan" },
      { "PAN", "Panjabi" },
      { "PA", "Panjabi" },
      { "PAP", "Papiamento" },
      { "PER", "Persian" },
      { "FA", "Persian" },
      { "FAS", "Persian" },
      { "PEO", "Persian" },
      { "PHN", "Phoenician" },
      { "PON", "Pohnpeian" },
      { "POL", "Polish" },
      { "PL", "Polish" },
      { "POR", "Portuguese" },
      { "PT", "Portuguese" },
      { "PT-BR", "Portuguese (Brazil)" },
      { "POB", "Portuguese (Brazil)" },
      { "PB", "Portuguese (Brazil)" },
      { "PUS", "Pushto" },
      { "PS", "Pushto" },
      { "QAA", "French" },
      { "QAD", "French" },
      { "QUE", "Quechua" },
      { "QU", "Quechua" },
      { "ROH", "Raeto-Romance" },
      { "RM", "Raeto-Romance" },
      { "RAJ", "Rajasthani" },
      { "RAP", "Rapanui" },
      { "RAR", "Rarotongan" },
      { "RUM", "Romanian" },
      { "RO", "Romanian" },
      { "RON", "Romanian" },
      { "ROM", "Romany" },
      { "RUN", "Rundi" },
      { "RN", "Rundi" },
      { "RUS", "Russian" },
      { "RU", "Russian" },
      { "SAM", "Samaritan Aramaic" },
      { "SMO", "Samoan" },
      { "SM", "Samoan" },
      { "SAD", "Sandawe" },
      { "SAG", "Sango" },
      { "SG", "Sango" },
      { "SAN", "Sanskrit" },
      { "SA", "Sanskrit" },
      { "SAT", "Santali" },
      { "SRD", "Sardinian" },
      { "SC", "Sardinian" },
      { "SAS", "Sasak" },
      { "SCO", "Scots" },
      { "SEL", "Selkup" },
      { "SRP", "Serbian" },
      { "SR", "Serbian" },
      { "SCC", "Serbian" },
      { "SRR", "Serer" },
      { "SHN", "Shan" },
      { "SNA", "Shona" },
      { "SN", "Shona" },
      { "III", "Sichuan Yi" },
      { "II", "Sichuan Yi" },
      { "SID", "Sidamo" },
      { "BLA", "Siksika" },
      { "SND", "Sindhi" },
      { "SD", "Sindhi" },
      { "SIN", "Sinhalese" },
      { "SI", "Sinhalese" },
      { "SMS", "Skolt Sami" },
      { "SLO", "Slovak" },
      { "SK", "Slovak" },
      { "SLK", "Slovak" },
      { "SLV", "Slovenian" },
      { "SL", "Slovenian" },
      { "SOG", "Sogdian" },
      { "SOM", "Somali" },
      { "SO", "Somali" },
      { "SON", "Songhai" },
      { "SNK", "Soninke" },
      { "NSO", "Sotho" },
      { "SOT", "Sotho" },
      { "ST", "Sotho" },
      { "SMA", "Southern Sami" },
      { "SUK", "Sukuma" },
      { "SUX", "Sumerian" },
      { "SUN", "Sundanese" },
      { "SU", "Sundanese" },
      { "SUS", "Susu" },
      { "SWA", "Swahili" },
      { "SW", "Swahili" },
      { "SSW", "Swati" },
      { "SS", "Swati" },
      { "SWE", "Swedish" },
      { "SV", "Swedish" },
      { "SYR", "Syriac" },
      { "TGL", "Tagalog" },
      { "TL", "Tagalog" },
      { "TAH", "Tahitian" },
      { "TY", "Tahitian" },
      { "TGK", "Tajik" },
      { "TG", "Tajik" },
      { "TMH", "Tamashek" },
      { "TAM", "Tamil" },
      { "TA", "Tamil" },
      { "TAT", "Tatar" },
      { "TT", "Tatar" },
      { "TEL", "Telugu" },
      { "TE", "Telugu" },
      { "TER", "Tereno" },
      { "TET", "Tetum" },
      { "THA", "Thai" },
      { "TH", "Thai" },
      { "TIB", "Tibetan" },
      { "BO", "Tibetan" },
      { "BOD", "Tibetan" },
      { "TIG", "Tigre" },
      { "TIR", "Tigrinya" },
      { "TI", "Tigrinya" },
      { "TEM", "Timne" },
      { "TIV", "Tiv" },
      { "TLI", "Tlingit" },
      { "TPI", "Tok Pisin" },
      { "TKL", "Tokelau" },
      { "TOG", "Tonga" },
      { "TON", "Tonga" },
      { "TO", "Tonga" },
      { "TSI", "Tsimshian" },
      { "TS", "Tsonga" },
      { "TSO", "Tsonga" },
      { "TSN", "Tswana" },
      { "TN", "Tswana" },
      { "TUM", "Tumbuka" },
      { "TUR", "Turkish" },
      { "TR", "Turkish" },
      { "OTA", "Turkish" },
      { "TUK", "Turkmen" },
      { "TK", "Turkmen" },
      { "TVL", "Tuvalu" },
      { "TYV", "Tuvinian" },
      { "TWI", "Twi" },
      { "TW", "Twi" },
      { "UGA", "Ugaritic" },
      { "UIG", "Uighur" },
      { "UG", "Uighur" },
      { "UKR", "Ukrainian" },
      { "UK", "Ukrainian" },
      { "UMB", "Umbundu" },
      { "URD", "Urdu" },
      { "UR", "Urdu" },
      { "UZB", "Uzbek" },
      { "UZ", "Uzbek" },
      { "VAI", "Vai" },
      { "VEN", "Venda" },
      { "VE", "Venda" },
      { "VIE", "Vietnamese" },
      { "VI", "Vietnamese" },
      { "VOL", "Volapuk" },
      { "VO", "Volapuk" },
      { "VOT", "Votic" },
      { "WAL", "Walamo" },
      { "WLN", "Walloon" },
      { "WA", "Walloon" },
      { "WAR", "Waray" },
      { "WAS", "Washo" },
      { "WEL", "Welsh" },
      { "CY", "Welsh" },
      { "CYM", "Welsh" },
      { "WOL", "Wolof" },
      { "WO", "Wolof" },
      { "XHO", "Xhosa" },
      { "XH", "Xhosa" },
      { "SAH", "Yakut" },
      { "YAO", "Yao" },
      { "YAP", "Yapese" },
      { "YID", "Yiddish" },
      { "YI", "Yiddish" },
      { "YOR", "Yoruba" },
      { "YO", "Yoruba" },
      { "ZND", "Zande" },
      { "ZAP", "Zapotec" },
      { "ZEN", "Zenaga" },
      { "ZHA", "Zhuang" },
      { "ZA", "Zhuang" },
      { "ZUL", "Zulu" },
      { "ZU", "Zulu" },
      { "ZUN", "Zuni" },
      { "NWC", "Newari" },
      { "TLH", "Klingon" },
      { "BYN", "Blin" },
      { "JBO", "Lojban" },
      { "CSB", "Kashubian" },
      { "CRH", "Crimean Turkish" },
      { "MYV", "Erzya" },
      { "MDF", "Moksha" },
      { "KRC", "Karachay-Balkar" },
      { "ADY", "Adyghe" },
      { "UDM", "Udmurt" },
      { "NOG", "Nogai" },
      { "HAT", "Haitian" },
      { "HT", "Haitian" },
      { "XAL", "Kalmyk" },
      { "UNK", UnknownLanguage }
    };

    #endregion

    /// <summary>
    /// Gets the language by LCID.
    /// </summary>
    /// <param name="lcid">The LCID.</param>
    /// <returns>Returns language name</returns>
    public static string GetLanguageByLcid(int lcid) =>
      LanguageLcids.TryGetValue((lcid & 0x3FF) + 0x400, out var result) ? result : UnknownLanguage;

    /// <summary>
    /// Gets language by the short language name.
    /// </summary>
    /// <param name="source">The short language name.</param>
    /// <returns>Returns language name.</returns>
    public static string GetLanguageByShortName(string source) =>
      Languages.TryGetValue(source, out var result) ? result : string.Empty;

    /// <summary>
    /// Gets LCID by short language name.
    /// </summary>
    /// <param name="source">The short language.</param>
    /// <returns>Returns LCID.</returns>
    public static int GetLcidByShortName(string source) =>
      Lcids.TryGetValue(source, out var result) ? result : 0;
  }
}
