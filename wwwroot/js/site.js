document.addEventListener('DOMContentLoaded', function () {
    const translateButton = document.getElementById("translator-translate");
    if (translateButton) translateButton.addEventListener('click', translateClick);

    const transliterateButton = document.getElementById("translator-transliterate");
    if (transliterateButton) transliterateButton.addEventListener('click', transliterateClick);

    if (translateButton) {
        document.addEventListener("selectionchange", onSelectionChange);
    }

    const addProducerButton = document.getElementById("db-add-producer");
    if (addProducerButton) addProducerButton.addEventListener('click', addProducerClick);
    loadProducers();
});
function loadProducers() {
    const container = document.getElementById("db-producers-container");
    if (!container) return;
    fetch("/api/db?type=Producer")
        .then(r => r.json())
        .then(j => {
            console.log(j);
//container.innerText = j
        } );
}
function addProducerClick() {
    const nameInput = document.querySelector("input[name='db-producer']");
    if (!nameInput) throw "input[name='db-producer'] Not found";
    fetch("/api/db", {
        method: "POST",
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ name: nameInput.value })
    }).then(r => r.text())
        .then(console.log);
}


var lastSelectionTimestamp = 0;
var delayedAction;
function onSelectionChange(e) {
    if (e.timeStamp - lastSelectionTimestamp < 300) {
        if (typeof delayedAction != 'undefined') {
            clearTimeout(delayedAction);
        }        
    }
    lastSelectionTimestamp = e.timeStamp;
    delayedAction = setTimeout(translateSelection, 1000);
    // console.log(e);
}
function translateSelection() {
    const check = document.getElementById("translator-selection");
    const check2 = document.getElementById("transliterator-selection");
    let text = document.getSelection().toString().trim();
    if (text.length > 0) {
        let [_, langFrom, langTo, __] = getTranslateData();
        if (check.checked) {
            fetch(`/api/translate?text=${text}&from=${langFrom.value}&to=${langTo.value}`)
                .then(r => r.json())
                .then(j => {
                    console.log(j);
                    alert(`${text} --> ${j[0].translations[0].text}`);
                });
        }
        else if (check2.checked) {
            const fromScript = langFrom.selectedOptions[0].getAttribute("data-script");
            if (!fromScript) {
                alert("Транслітерація цієї мови не підтримується");
                return;
            }
            const toScript = "Latn";
            fetch(
                `/api/translate?text=${text}&from=${langFrom.value}&fromScript=${fromScript}&toScript=${toScript}`, {
                method: "POST"})
                .then(r => r.json())
                .then(j => {
                    console.log(j);
                    alert(`${text} ==> ${j[0].text}`);
                });
        }
    }
    delayedAction = undefined;
    /* Д.З. Перекладач/транслітератор в автоматичному режимі.
    Реалізувати одночасну роботу перекладача та транслітератора:
    якщо вибрані обидві опції то включати до повідомлення як переклад, 
    так і транслітерацію

    모든 --> увесь
    모든 ==> modeun

    */
}

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
            const data = j[0];
            if (typeof data['detectedLanguage'] != 'undefined') {
                // автоматичне визначення мови джерела
                const lang = data['detectedLanguage']['language']
                langFrom.value = lang;
                // if (!langFrom.selectedOptions[0].innerText.endsWith('(detected)')) {
                //     langFrom.selectedOptions[0].innerText += ' (detected)';
                // }
                let opts = langFrom.querySelectorAll(`option[disabled]`);
                for (let op of opts) {
                    langFrom.removeChild(op);
                }
                let opt = document.createElement('option');
                opt.disabled = true;
                opt.selected = true;
                opt.value = lang;
                opt.innerText = langFrom
                    .querySelector(`option[value=${lang}]`)
                    .innerText + ' (detected)';
                langFrom.options.add(opt);

            }
            output.value = data.translations[0].text;
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