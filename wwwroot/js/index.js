'use strict';

let baseUrl = "https://album-search.golf1052.com/api";
// let baseUrl = "https://1c5e66f5aa56.ngrok.io/api";
let accessToken = '';
let state = '';
let userAlbums = null;
let albumsMap = null;
let isDesktop = true;

function getAuthorizeUrl() {
    return fetch(`${baseUrl}/spotify/authorize`, {
        mode: 'cors'
    })
    .then(response => {
        return response.text();
    })
    .catch(err => {
        console.error(err);
        addAlert(err);
    });
}

function processRedirect() {
    let splitUrl = window.location.href.split('#');
    if (splitUrl.length > 1) {
        let params = new URLSearchParams(splitUrl[1]);
        if (params.has('accessToken') && params.has('state')) {
            accessToken = params.get('accessToken');
            state = params.get('state');
            window.location.hash = '';
            setupNavbar();
            startLoading();
            processAlbums();
        }
    }
}

function processAlbums() {
    let params = new URLSearchParams();
    params.set('accessToken', accessToken);
    params.set('state', state);
    fetch(`${baseUrl}/spotify/ProcessAlbums`, {
        method: 'POST',
        body: params
    })
    .then(response => {
        return response.json();
    })
    .then(data => {
        userAlbums = data.albums;
        // sort albums by album name
        userAlbums.sort((a, b) => {
            return a.albumName.localeCompare(b.albumName);
        });
        albumsMap = new Map();
        userAlbums.forEach(album => {
            albumsMap.set(album.albumId, album)
        });
        displayAlbums(userAlbums);
    })
    .catch(err => {
        console.error(err);
        addAlert(err);
    });
}

function search(searchQuery) {
    let body = {
        accessToken: accessToken,
        state: state,
        albumIds: userAlbums.map(album => album.albumId),
        searchQuery: searchQuery
    };

    return fetch(`${baseUrl}/spotify/Search`, {
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
        return data;
    })
    .catch(err => {
        console.error(err);
        addAlert(err);
    });
}

function displayAlbums(albums) {
    hideLoadingText();
    stopLoading();
    let contentDiv = $('#content');
    let cardDiv = null;
    for (let i = 0; i < albums.length; i++) {
        if (isDesktop) {
            if (i % 6 == 0) {
                cardDiv = createAppend(contentDiv, 'div')
                    .addClass('card-deck justify-content-center')
                    .css('margin-bottom', '10px')
            }
        } else {
            cardDiv = contentDiv;
        }
        
        let album = albums[i];
        let card = createCard(album);
        cardDiv.append([card]);
    }
}

function createCard(album) {
    if (isDesktop) {
        return createDesktopCard(album);
    } else {
        return createMobileCard(album);
    }
}

function createDesktopCard(album) {
    let card = createElement('div')
        .addClass('card')
        .css('max-width', '25rem');
        // .width('20rem');
    let albumLink = createAppend(card, 'a')
        .attr('href', album.albumId);
    createAppend(albumLink, 'img')
        .addClass('card-img-top')
        .attr('src', album.imageUrl);
    let cardBody = createAppend(card, 'div')
        .addClass('card-body');
    createAppend(cardBody, 'h4')
        .addClass('card-title')
        .text(album.albumName);
    createAppend(cardBody, 'p')
        .addClass('card-text')
        .text(`by ${album.artistName}`);
    return card;
}

function createMobileCard(album) {
    let card = createElement('div')
        .addClass('card')
        .css('margin', '10px 0px');
    let row = createAppend(card, 'div')
        .addClass('row no-gutters');
    let imageCol = createAppend(row, 'div')
        .addClass('col-3');
    let albumLink = createAppend(imageCol, 'a')
        .attr('href', album.albumId);
    createAppend(albumLink, 'img')
        .addClass('card-img')
        .attr('src', album.imageUrl);
    let textCol = createAppend(row, 'div')
        .addClass('col-9');
    let cardBody = createAppend(textCol, 'div')
        .addClass('card-body')
        .css('padding-top', '0px')
        .css('padding-bottom', '0px')
    createAppend(cardBody, 'h5')
        .addClass('card-title')
        .text(album.albumName);
    createAppend(cardBody, 'p')
        .addClass('card-text')
        .text(`by ${album.artistName}`);
    return card;
}

function createAppend(obj, tag) {
    let element = createElement(tag);
    obj.append([element]);
    return element;
}

function createElement(tag) {
    return $(`<${tag}></${tag}>`);
}

function addAlert(text) {
    let alert = $('<div></div>').addClass('alert alert-danger').text(text);
    $('#alert-div').append([alert]);
}

function removeAlerts() {
    $('#alert-div').children().remove();
}

function setupPage() {
    isDesktop = window.screen.availWidth >= 576;
}

function setupNavbar() {
    let navbar;
    if (isDesktop) {
        navbar = createElement('nav')
            .addClass('navbar fixed-top navbar-expand-sm navbar-light bg-light');
        $('#header').append([navbar]);
        $('#container').css('margin-top', '60px');
    } else {
        navbar = createElement('nav')
            .addClass('navbar fixed-bottom navbar-expand-sm navbar-light bg-light');
        $('#footer').append([navbar]);
        $('#container').css('margin-bottom', '60px');
    }

    createAppend(navbar, 'input')
        .attr('id', 'searchInput')
        .prop('type', 'search')
        .addClass('form-control');

    document.getElementById('searchInput').addEventListener('change', function() {
        if (this.value.trim() != '') {
            startLoading();
            search(this.value.trim())
            .then(data => {
                let foundAlbums = data.albumIds.map(albumId => {
                    return albumsMap.get(albumId)
                });
                $('#content').children().remove();
                displayAlbums(foundAlbums);
            });
        } else {
            $('#content').children().remove();
            displayAlbums(userAlbums);
        }
    });

    $('#landing').prop('hidden', true);   
}

function startLoading() {
    $('#searchInput').prop('disabled', true);
    $('#loading').prop('hidden', false);
}

function hideLoadingText() {
    $('#loading-text').prop('hidden', true);
}

function stopLoading() {
    $('#searchInput').prop('disabled', false);
    $('#loading').prop('hidden', true);
}

setupPage();

document.getElementById('authorizeButton').addEventListener("click", function() {
    $('#authorizeButton').prop('disabled', true);
    getAuthorizeUrl().then(url => {
        if (url) {
            window.location.href = url;
        }
    });
});

processRedirect();
