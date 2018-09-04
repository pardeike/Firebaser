# Firebaser

A lightweight mono 3.5 compatible connector to the legacy firebase api.

## Firebaser uses the Firebase Realtime Database and in particular its REST API defined in
- https://firebase.google.com/docs/database/
- https://firebase.google.com/docs/database/rest/start

Keys (and therefore object fields) must follow the following rule:
`If you create your own keys, they must be UTF-8 encoded, can be a maximum of 768 bytes, and cannot contain., $, #, [, ], /, or ASCII control characters 0-31 or 127.`

Author: Andreas Pardeike

---

Based on the work of https://github.com/mgholam/fastJSON and modified to work under mono 3.5