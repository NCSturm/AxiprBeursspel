function setGebruikerGeld() {
    $.ajax({
        url: '/Account/GetGebruikerGeld',
        type: "POST",
        success: function(data) {
            $('#waarde-houder').text("€" + data)
        },
        error: function(xhr, status){
        }
    });
}

showdown.setOption('simpleLineBreaks', 'true');

function markdownify(s) {
    var converter = new showdown.Converter();
    return converter.makeHtml(s);
}

window.onload = function(){
    setGebruikerGeld();
};