/*
   Copyright (C) 2012 Red Hat, Inc.

   This library is free software; you can redistribute it and/or
   modify it under the terms of the GNU Lesser General Public
   License as published by the Free Software Foundation; either
   version 2.1 of the License, or (at your option) any later version.

   This library is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
   Lesser General Public License for more details.

   You should have received a copy of the GNU Lesser General Public
   License along with this library; if not, see <http://www.gnu.org/licenses/>.
*/

#ifndef H_SPICE_COMMON_LOG
#define H_SPICE_COMMON_LOG

#include <stdarg.h>
#include <stdio.h>

#include "macros.h"

SPICE_BEGIN_DECLS

#ifdef SPICE_LOG_DOMAIN
#error Do not use obsolete SPICE_LOG_DOMAIN macro, is currently unused
#endif

/* FIXME: name is misleading, this aborts.. */
#define spice_return_if_fail(x) {                          \
    if (x) { } else {                                           \
        return;                                                         \
    }                                                                   \
}

/* FIXME: name is misleading, this aborts.. */
#define spice_return_val_if_fail(x, val) {                 \
    if (x) { } else {                                           \
        return (val);                                                   \
    }                                                                   \
}

SPICE_END_DECLS

#endif // H_SPICE_COMMON_LOG
