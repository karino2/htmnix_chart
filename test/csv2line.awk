#!/usr/bin/env awk -f

BEGIN {
    FS =","
    print "{"
    print "\"type\":\"line\","
    print "\"data\": {"
    print "  \"datasets\": [{"
    print "    \"data\": ["
}

NR == 2 {
    printf("{\"x\":\"%s\", \"y\": %s}\n", $1, $2)
}
NR > 2 {
    printf(",{\"x\":\"%s\", \"y\": %s}\n", $1, $2)
}


END {
print "     ]"
print "   }]"
print " }"
print "}"   
}