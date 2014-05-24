#!/bin/bash
find . -iname '*.cs' -and -not -iname 'AssemblyInfo.cs' -exec du -k {} \; | sed 's/^[ \t]*//' | cut -d '	' -f 1 | paste -s -d '+' - | bc
