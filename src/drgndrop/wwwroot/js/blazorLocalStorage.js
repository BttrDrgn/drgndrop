window.localStore = {
    write: (key, value) => {
        localStorage.setItem(key, value);
    },
    read: (key) => {
        return localStorage.getItem(key);
    },
    delete: (key) => {
        localStorage.removeItem(key);
    },
    clear: () => {
        localStorage.clear();
    }
}