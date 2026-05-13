const langData = {
    en: { name: "English", flag: "https://flagcdn.com/gb.svg" },
    ru: { name: "Русский", flag: "https://flagcdn.com/ru.svg" },
    de: { name: "Deutsch", flag: "https://flagcdn.com/de.svg" },
    es: { name: "Español", flag: "https://flagcdn.com/es.svg" },
    ko: { name: "한국어", flag: "https://flagcdn.com/kr.svg" }
};

function getCookie(name) {
    const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
    return match ? match[2] : null;
}


function changeLanguage(selectedLang) {
    document.cookie = `preferredLanguage=${selectedLang}; path=/; max-age=31536000`;
    updateLanguageUI(selectedLang);
    location.reload();
}

function updateLanguageUI(lang) {
    const data = langData[lang] || langData.en;

    const flagElem = document.getElementById('currentLangFlag');
    const textElem = document.getElementById('currentLangText');

    if (flagElem) {
        flagElem.src = data.flag;
    }

    if (textElem) {
        textElem.textContent = data.name;
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const savedLang = getCookie('preferredLanguage') || 'en';
    updateLanguageUI(savedLang);
});
