#!/bin/bash
find . -iname '*.cs' -and -not -iname 'AssemblyInfo.cs' -exec wc -c {} \; | sed 's/^[ \t]*//' | cut -d ' ' -f 1 | paste -s -d '+' - | bc
