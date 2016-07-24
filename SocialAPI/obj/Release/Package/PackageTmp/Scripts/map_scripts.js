$(document).ready(function () {
    GetMap();
});

function setIcons() {
  
    map = new google.maps.Map(document.getElementById("canvas"), {
        zoom: 16,
        center: new google.maps.LatLng(GeocodeInfo.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos.split(' ')[1],
                GeocodeInfo.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos.split(' ')[0]),
        mapTypeId: google.maps.MapTypeId.G_NORMAL_MAP
    });
    refresh_map();

    for (var i in GeocodeInfo.response.GeoObjectCollection.featureMember) {

        var target = new google.maps.LatLng(GeocodeInfo.response.GeoObjectCollection.featureMember[i].GeoObject.Point.pos.split(' ')[1],
                GeocodeInfo.response.GeoObjectCollection.featureMember[i].GeoObject.Point.pos.split(' ')[0]);

        geoMarker = new google.maps.Marker({
            position: target,
            map: map,
            title: GeocodeInfo.response.GeoObjectCollection.featureMember[i].GeoObject.metaDataProperty.GeocoderMetaData.text,
            animation: google.maps.Animation.DROP
        });

        geoMarker.info = new google.maps.InfoWindow({
            content: "<div><font size='3'>" + GeocodeInfo.response.GeoObjectCollection.featureMember[i].GeoObject.metaDataProperty.GeocoderMetaData.text + "</font><br/><font size='2'>Страна: " + GeocodeInfo.response.GeoObjectCollection.featureMember[i].GeoObject.metaDataProperty.GeocoderMetaData.AddressDetails.Country.CountryName + "<br/>Код страны: " + GeocodeInfo.response.GeoObjectCollection.featureMember[i].GeoObject.metaDataProperty.GeocoderMetaData.AddressDetails.Country.CountryNameCode + "</font><br/><div style='float: right'><font size='1'>Кликните дважды, чтобы удалить маркер</font></div></div>",
            disableAutoPan: true
        });

        geoMarker.addListener('click', function () {
            this.info.open(map, this);
        });

        geoMarker.addListener('dblclick', function () {
            this.setMap(null);
        });
    }
}
		
function GetMap() {
    google.maps.visualRefresh = true;

    var Usatu = new google.maps.LatLng(54.725038563663304, 55.94118621303003);

    var mapOptions = {
        zoom: 16,
        center: Usatu,
        mapTypeId: google.maps.MapTypeId.G_NORMAL_MAP
    };

    map = new google.maps.Map(document.getElementById("canvas"), mapOptions);

    $.getJSON('@Url.Action("GetData","Home")', function (data) {
        $.each(data, function (i, item) {
            var marker = new google.maps.Marker({
                position: new google.maps.LatLng(parseFloat(item.latitude), parseFloat(item.longitude)),
                map: map,
                title: item.name
            });

            var infowindow = new google.maps.InfoWindow({
                content: "<div style='width: 250px; margin-right: 20px'><font size='3'>" + item.name + "</font><br/><font size='2'>" + item.description + "</font><br/><br/><font size='1'>Количество пользователей: " + item.members_count + "</font><br/><br/><div style='text-align: center'><img src='" + item.photo_big + "' width='70%'></img></div></div>"
            });

            google.maps.event.addListener(marker, 'click', function () {
                infowindow.open(map, marker);
            });
        })
    });
}