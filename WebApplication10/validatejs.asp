﻿<script type="text/javascript">

    function httpGet(theUrl) {
        var xmlHttp = new XMLHttpRequest();
        xmlHttp.open("GET", theUrl, false); // false for synchronous request
        xmlHttp.send(null);
        return xmlHttp.responseText;
    }
</script>

<script type="text/javascript">
    function lol() {
        alert("lol");
    }
</script>