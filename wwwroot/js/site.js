

function markdownify(s) {
    s = s.replace(new RegExp("&#xD;", 'g'), "\r");
    s = s.replace(new RegExp("&#xA;", 'g'), "\n");
    console.log(s);
    showdown.setOption('simpleLineBreaks', 'true');
    var converter = new showdown.Converter();
    return converter.makeHtml(s);
}

function gaNaarBeurs(id) {
    window.document.location = "/Beurzen/Beurs/" + id;
}