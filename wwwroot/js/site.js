document.addEventListener('DOMContentLoaded', function () {
    const translateButton = document.getElementById("translator-translate");
    if (translateButton) translateButton.addEventListener('click', translateClick);
});


function translateClick() {
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

    fetch(`/api/translate?text=${text}&from=${langFrom.value}&to=${langTo.value}`)
        .then(r => r.json())
        .then(j => {
            console.log(j);
            output.value = j[0].translations[0].text;
        });

    // output.value = input.value + ' ' + langFrom.value + ' ' + langTo.value;
    // "translator-switch" "translator-lang-from" "translator-lang-to"
}  