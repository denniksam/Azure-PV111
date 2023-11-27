document.addEventListener('DOMContentLoaded', function () {
    const translateButton = document.getElementById("translator-translate");
    if (translateButton) translateButton.addEventListener('click', translateClick);

    const transliterateButton = document.getElementById("translator-transliterate");
    if (transliterateButton) transliterateButton.addEventListener('click', transliterateClick);


});

function getTranslateData() {
    const input = document.getElementById("translator-input");
    if (!input) throw "#translator-input not found";
    const text = input.value.trim();
    if (text.length == 0) {
        alert("Введіть текст для перекладу");
        input.focus();
        return;
    }
    const output = document.getElementById("translator-output");
    if (!output) throw "#translator-output not found";

    const langFrom = document.getElementById("translator-lang-from");
    if (!langFrom) throw "#translator-lang-from not found";

    const langTo = document.getElementById("translator-lang-to");
    if (!langTo) throw "#translator-lang-to not found";

    return [text, langFrom, langTo, output];
}

function translateClick() {    
    let [text, langFrom, langTo, output] = getTranslateData();
    fetch(`/api/translate?text=${text}&from=${langFrom.value}&to=${langTo.value}`)
        .then(r => r.json())
        .then(j => {
            console.log(j);
            output.value = j[0].translations[0].text;
        });

    // output.value = input.value + ' ' + langFrom.value + ' ' + langTo.value;
    // "translator-switch" "translator-lang-from" "translator-lang-to"
}  

function transliterateClick() {
    let [_, __, langTo, output] = getTranslateData();
    const text = output.value;
    if (!text) {
        alert("Спочатку виконайте переклад");
        return;
    }
    const fromScript = langTo.selectedOptions[0].getAttribute("data-script");
    if (!fromScript) {
        alert("Транслітерація цієї мови не підтримується");
        return;
    }
    const toScript = "Latn";

    fetch(
        `/api/translate?text=${text}&from=${langTo.value}&fromScript=${fromScript}&toScript=${toScript}`, {
            method: "POST"
        })
        .then(r => r.text())
        .then(j => {
            console.log(j);
            // output.value = j[0].translations[0].text;
        });
}