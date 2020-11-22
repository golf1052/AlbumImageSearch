let baseUrl = "https://f8a6784dcfb6.ngrok.io/api";
let accessToken = '';
let userAlbums = null;

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
        return response.json();
    })
    .then(data => {
        userAlbums = data.albums;
        console.log('ready');
    })
    .catch(err => {
        console.error(err);
    });
}

function search(searchQuery) {
    let body = {
        accessToken: accessToken,
        albumIds: userAlbums.map(album => album.albumId),
        searchQuery: searchQuery
    };

    fetch(`${baseUrl}/spotify/Search`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(body)
    })
    .then(response => {
        return response.json();
    })
    .then(data => {
        console.log(data);
        return data;
    })
    .catch(err => {
        console.error(err);
    });
}

document.getElementById('authorizeButton').addEventListener("click", function() {
    getAuthorizeUrl().then(url => {
        if (url) {
            window.location.href = url;
        }
    });
});

document.getElementById('searchInput').addEventListener('change', function() {
    if (this.value.trim() != '') {
        search(this.value.trim())
        .then(data => {
        });
    }
});

processRedirect();
