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

    const linkButton = document.getElementById("db-button-link");
    if (linkButton) linkButton.addEventListener('click', linkButtonClick);

    const addProductButton = document.getElementById("db-add-product");
    if (addProductButton) addProductButton.addEventListener('click', addProductClick);

});
function addProductClick() {
    const productsContainer = document.getElementById("db-product-container");
    if (!productsContainer) throw "#db-product-container not found";
    const producerId = productsContainer.getAttribute('data-producer-id');
    if (!producerId) {
        alert("Виберіть виробника");
        return;
    }
    console.log(producerId);
    const nameInput = document.querySelector('[name="db-product-name"]');
    if (!nameInput) throw "db-product-name not found";
    const yearInput = document.querySelector('[name="db-product-year"]');
    if (!yearInput) throw "db-product-year not found";
    fetch('/api/db', {
        method: 'ADD',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            producerId: producerId,
            name: nameInput.value,
            year: yearInput.value
        })
    }).then(r => r.text())
        .then(console.log);
}
function linkButtonClick() {
    fetch('/api/db', { method: 'LINK' })
        .then(r => r.text())
        .then(console.log);
}
function loadProducers() {
    const container = document.getElementById("db-producers-container");
    if (!container) return;
    fetch("/api/db?type=Producer")
        .then(r => r.json())
        .then(j => {
            const table = document.createElement('table');
            const tbody = document.createElement('tbody');
            for (let item of j) {
                let tr, td, tn;
                tr = document.createElement('tr');
                tr.setAttribute('producer-id', item.id);

                // radio button
                td = document.createElement('td');
                let rb = document.createElement('input');
                rb.setAttribute('type', 'radio');
                rb.setAttribute('name', 'rb-producer');
                rb.setAttribute('value', item.id);
                rb.addEventListener('change', rbProducerChanged);
                tr.appendChild(rb);

                td = document.createElement('td');
                td.setAttribute('data-name', '');
                tn = document.createTextNode(item.name);
                td.appendChild(tn);
                tr.appendChild(td);

                // delete button
                td = document.createElement('td');
                let btn = document.createElement('button');
                btn.classList.add('btn', 'btn-danger');
                // <i class="bi bi-trash-fill"></i>
                let i = document.createElement('i');
                i.classList.add('bi', 'bi-trash-fill');
                btn.appendChild(i);
                btn.addEventListener('click', deleteProducerClick);
                td.appendChild(btn);
                tr.appendChild(td);

                // edit button
                td = document.createElement('td');
                btn = document.createElement('button');
                btn.classList.add('btn', 'btn-warning');
                // <i class="bi bi-pen-fill"></i>
                i = document.createElement('i');
                i.classList.add('bi', 'bi-pen-fill');
                btn.appendChild(i);
                btn.addEventListener('click', editProducerClick);
                td.appendChild(btn);
                tr.appendChild(td);

                tbody.appendChild(tr);
            }
            table.appendChild(tbody);
            table.className = "table";
            container.innerHTML = "";
            container.appendChild(table);
            console.log(j);
//container.innerText = j
        } );
}
function rbProducerChanged(e) {
    loadProducts(e.target.value);
}
function loadProducts(producerId) {
    const productsContainer = document.getElementById("db-product-container");
    if (!productsContainer) throw "#db-product-container not found";
    productsContainer.setAttribute('data-producer-id', producerId);
}
function findProducerData(e) {
const idCarrier = e.target.closest('[producer-id]');
    if (!idCarrier) throw "[producer-id] not found";
    const producerId = idCarrier.getAttribute('producer-id');
    const nameCarrier = idCarrier.querySelector('[data-name]');
    return { producerId, nameCarrier, idCarrier };
}
function editProducerClick(e) {
    const { producerId, nameCarrier, idCarrier } = findProducerData(e);
    const newName = prompt("Введіть нову назву", nameCarrier.innerText);
    if (newName != nameCarrier.innerText
        && newName !== ""
        && newName !== null
    ) {
        fetch(`/api/db?producerId=${producerId}&newName=${newName}`, {
            method: 'PUT'
        })
            .then(r => r.json())
            .then(j => {
                console.log(j);
                if (j.status == 200) {
                    nameCarrier.innerText = newName;
                }
                else {
                    alert("Помилка при внесені змін");
                }
            });
    }
    else {
        alert("Зміни скасовані");
    }
}
function deleteProducerClick(e) {
    const { producerId, nameCarrier, idCarrier } = findProducerData(e);
    if (confirm("Підтверджуєте видалення " + nameCarrier.innerText)) {
        console.log("To delete " + producerId);
        fetch(`/api/db?producerId=${producerId}`, {
            method: "DELETE"
        }).then(r => r.json()).then(j => {
            if (j.status == 204) {  // success
                idCarrier.remove();
            }
            else {
                alert("Щось пішло не так...");
            }
        });

    }
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
        .then(_ => location.reload());
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