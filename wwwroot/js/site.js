
showdown.setOption('simpleLineBreaks', 'true');

function markdownify(s) {
    var converter = new showdown.Converter();
    return converter.makeHtml(s);
}