let baseUrl = "https://5744a3a4145b.ngrok.io/api";
let accessToken = '';

document.getElementById('authorizeButton').addEventListener("click", function() {
    getAuthorizeUrl().then(url => {
        if (url) {
            window.location.href = url;
        }
    });
});

function getAuthorizeUrl() {
    return fetch(`${baseUrl}/spotify/authorize`, {
        mode: 'cors'
    })
    .then(response => {
        return response.text();
    })
    .catch(err => {
        console.error(err);
    });
}

function processRedirect() {
    let splitUrl = window.location.href.split('?');
    if (splitUrl.length > 1) {
        let params = new URLSearchParams(splitUrl[1]);
        if (params.has('code')) {
            fetch(`${baseUrl}/spotify/authorize`, {
                method: 'POST',
                body: params
            })
            .then(response => {
                return response.text();
            })
            .then(access_code => {
                accessToken = access_code;
                processAlbums();
            });
        }
    }
}

function processAlbums() {
    let params = new URLSearchParams();
    params.set('accessToken', accessToken);
    fetch(`${baseUrl}/spotify/ProcessAlbums`, {
        method: 'POST',
        body: params
    })
    .then(response => {
    })
    .catch(err => {
        console.error(err);
    });
}

processRedirect();
