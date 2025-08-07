#!/bin/bash

read WIDTH < <(identify -format "%w" "$1")

NEW_SZ=$(( WIDTH + 1 ))

convert "$1" \
  -alpha off \
  -colorspace Gray \
  -depth 16 \
  -endian lsb \
  -gravity northwest \
  -background black \
  -extent "${NEW_SZ}x${NEW_SZ}" \
  gray:heightmap.raw
