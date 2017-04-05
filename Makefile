#!/usr/bin/make -f

# NOTE: This Makefile requires Mono and MonoGame. And does not support shitty
#       operating systems (e.g. Windows). :-)

#---------------------------------------
# CONSTANTS
#---------------------------------------

# Paths
BINDIR     = bin
CONTENTDIR = Content
OBJDIR     = obj
SRCDIR     = Source

# Mono C# Compiler
COMPILER = mcs
FLAGS    = -debug+ -define:DEBUG -target:winexe
LIBPATHS = $(MONOGAME_PATH)
LIBS     = MonoGame.Framework.dll
TARGET   = Program.exe

# MonoGame Content Builder
CONTENTFILE = content.mgcb

#---------------------------------------
# INITIALIZATION
#---------------------------------------

# Linux and macOS have different paths to the MonoGame library files, so make
# sure to set them up properly. No Windows support here, lol!
OS := $(shell uname)

ifeq "$(OS)" "Linux"
MONOGAME_PATH = /usr/lib/mono/xbuild/MonoGame/v3.0
endif

ifeq "$(OS)" "Darwin"
MONOGAME_PATH = /Library/Frameworks/MonoGame.framework/Current
endif

MONOGAME_PATH := $(MONOGAME_PATH)/Assemblies/DesktopGL

#---------------------------------------
# TARGETS
#---------------------------------------

# Make sure we can't break these targets by creating weirdly named files.
.PHONY: all clean libs run

# Default target.
all: compile content libs

clean:
	rm -rf $(CONTENTFILE) $(BINDIR) $(OBJDIR)

libs:
	mkdir -p $(BINDIR)
	-cp -nr $(MONOGAME_PATH)/* $(BINDIR)

run: all
	cd $(BINDIR); \
	mono $(TARGET)

#-------------------
# MONO
#-------------------

# Always recompile. Makes it easier to work on the project.
.PHONY: $(BINDIR)/$(TARGET) compile

$(BINDIR)/$(TARGET):
	mkdir -p $(BINDIR)
	$(COMPILER) $(FLAGS)                        \
	            $(addprefix -lib:, $(LIBPATHS)) \
	            $(addprefix -r:, $(LIBS))       \
	            -out:$(BINDIR)/$(TARGET)        \
	            -recurse:$(SRCDIR)/*.cs

compile: $(BINDIR)/$(TARGET)

#-------------------
# MONOGAME
#-------------------

# Find all content to build with MonoGame Content Builder.
CONTENT := $(shell find $(CONTENTDIR) -type f)

# Kind of a hack to build content easily.
.PHONY: $(CONTENTDIR)/*/* pre-content content

$(CONTENTDIR)/Models/*.fbx:
	@echo /build:$@ >> $(CONTENTFILE)

$(CONTENTDIR)/Textures/*.png:
	@echo /build:$@ >> $(CONTENTFILE)

pre-content:
	@echo /compress                   > $(CONTENTFILE)
	@echo /intermediateDir:$(OBJDIR) >> $(CONTENTFILE)
	@echo /outputDir:$(BINDIR)       >> $(CONTENTFILE)
	@echo /quiet                     >> $(CONTENTFILE)

content: pre-content $(CONTENT)
	mkdir -p $(BINDIR)
	mgcb -@:$(CONTENTFILE)
	rm -f $(CONTENTFILE)
