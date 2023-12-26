window.cookies = {
    write: (key, value, days) => {
        var expires;
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.toGMTString();
        }
        else {
            expires = "";
        }
        document.cookie = key + "=" + value + expires + "; path=/";
        console.log(document.cookie);
    },
    read: () => {
        return document.cookie;
    }
}