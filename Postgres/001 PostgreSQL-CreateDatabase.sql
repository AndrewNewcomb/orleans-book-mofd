
CREATE DATABASE orleansbook;

CREATE ROLE orleansbookgrp;
REVOKE ALL ON DATABASE orleansbook FROM public;  -- shut out the general public
GRANT CONNECT ON DATABASE orleansbook TO orleansbookgrp;  -- since we revoked from public

\c orleansbook;

GRANT USAGE ON SCHEMA public TO orleansbookgrp;

GRANT ALL ON ALL TABLES IN SCHEMA public TO orleansbookgrp;
GRANT ALL ON ALL SEQUENCES IN SCHEMA public TO orleansbookgrp; -- don't forget those



