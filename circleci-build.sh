#!/bin/bash
make || exit 1
cd v4Tests || exit 1
make || exit 1
cd ../v4Tests-32 || exit 1
make || exit 1
