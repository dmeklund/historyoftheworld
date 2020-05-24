import json

import mysql.connector
import flask
from flask import Flask, request
from flask_restful import Resource, Api, reqparse

app = Flask(__name__)
api = Api(app)

parser = reqparse.RequestParser()
parser.add_argument("_southWest")
parser.add_argument("_northEast")

def project(val, minval, maxval):
    span = maxval - minval
    assert span > 0
    while val < minval:
        val += span
    while val > maxval:
        val -= span
    return val

@app.route("/")
def index():
    return flask.send_from_directory("static", "mapapp.html")

@app.route("/static/<path:path>")
def send_static(path):
    return flask.send_from_directory("static", path)

@app.route("/js/<path:path>")
def send_js(path):
    return flask.send_from_directory("js", path)

class Events(Resource):
    def post(self):
        # args = parser.parse_args()
        # southwest = json.loads(args['_southWest'])
        # northeast = json.loads(args['_northEast'])
        southwest = request.json['_southWest']
        northeast = request.json['_northEast']
        minlng = southwest['lng']
        maxlng = northeast['lng']
        if maxlng - minlng > 360:
            minlng = 0
            maxlng = 360
        else:
            pass
        minlat = project(southwest['lat'], -180, 180)
        minlng = project(southwest['lng'], -180, 180)
        maxlat = project(northeast['lat'], -180, 180)
        maxlng = project(northeast['lng'], -180, 180)
        db = mysql.connector.connect(
            host="localhost",
            user="hotw",
            passwd="hotw",
            database="hotw",
            auth_plugin='mysql_native_password'
        )
        cursor = db.cursor()
        args = {
            "minlat": minlat,
            "minlng": minlng,
            "maxlat": maxlat,
            "maxlng": maxlng
        }
        query = """
            SELECT id, title, eventtype, lat, lng 
            FROM events 
            WHERE lat BETWEEN %(minlat)f AND %(maxlat)f
            AND lng BETWEEN %(minlng)f AND %(maxlng)f
            LIMIT 100
        """ % args
        print(query)
        cursor.execute(query)
        allevents = []
        for result in cursor.fetchall():
            allevents.append({
                'id': result[0],
                'title': result[1] + result[2], 
                'lat': result[3], 
                'lng': result[4]}
            )
        print("Returning: {}".format(json.dumps(allevents)))
        return allevents

api.add_resource(Events, '/events')

if __name__ == '__main__':
    app.run(debug=True)