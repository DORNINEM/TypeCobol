﻿*  /////////////////////////////////
* // INSPECT Format 1 (TALLYING)
* CHARACTERS
INSPECT x TALLYING i FOR CHARACTERS
INSPECT x TALLYING i FOR CHARACTERS j FOR CHARACTERS k FOR CHARACTERS
INSPECT x TALLYING i FOR CHARACTERS BEFORE b AFTER c j FOR CHARACTERS BEFORE d k FOR CHARACTERS AFTER 8
* ALL, LEADING
INSPECT x TALLYING i FOR ALL x
INSPECT x TALLYING i FOR LEADING y
INSPECT x TALLYING i FOR ALL x j FOR LEADING y
* both
INSPECT x TALLYING i FOR CHARACTERS j FOR ALL x k FOR CHARACTERS l FOR LEADING y

*  //////////////////////////////////
* // INSPECT Format 2 (REPLACING)
* CHARACTERS BY
INSPECT x REPLACING CHARACTERS BY i
INSPECT x REPLACING CHARACTERS BY i CHARACTERS BY j CHARACTERS BY k
INSPECT x REPLACING CHARACTERS BY i AFTER a CHARACTERS BY j BEFORE b CHARACTERS BY k AFTER a BEFORE b
* ALL, LEADING, FIRST
INSPECT x REPLACING ALL x BY y
INSPECT x REPLACING FIRST z BY 1
INSPECT x REPLACING FIRST 2 BY y
INSPECT x REPLACING LEADING 2 BY 1
INSPECT x REPLACING ALL x BY y FIRST z BY 1 FIRST 2 BY y LEADING 2 BY 1
* both
INSPECT x REPLACING CHARACTERS BY i ALL x BY y CHARACTERS BY j FIRST z BY y CHARACTERS BY k LEADING 2 BY 1

*  ////////////////////////////////////////////////
* // INSPECT Format 3 (TALLYING then REPLACING)
INSPECT x TALLYING i FOR CHARACTERS REPLACING CHARACTERS BY i
INSPECT x TALLYING i FOR CHARACTERS j FOR ALL x k FOR CHARACTERS l FOR LEADING y
          REPLACING CHARACTERS BY i ALL x BY y CHARACTERS BY j FIRST z BY y CHARACTERS BY k LEADING 2 BY 1