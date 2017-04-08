#!/usr/bin/make -f

# NOTE: This Makefile requires Mono and MonoGame. And does not support shitty
#       operating systems (e.g. Windows). :-)

#---------------------------------------
# CONSTANTS
#---------------------------------------

# Common config
BINDIR = bin

# Engine config
ENGINE_COMPILER   = mcs
ENGINE_FLAGS      = -debug+ -define:DEBUG -target:library
ENGINE_LIBPATHS   = $(MONOGAME_PATH)
ENGINE_LIBS       = MonoGame.Framework.dll
ENGINE_SRCDIR     = Source/EngineName
ENGINE_TARGET     = EngineName.dll

# Game config
GAME_COMPILER   = mcs
GAME_CONTENTDIR = Content
GAME_FLAGS      = -debug+ -define:DEBUG -target:winexe
GAME_LIBPATHS   = $(BINDIR) $(MONOGAME_PATH)
GAME_LIBS       = EngineName.dll MonoGame.Framework.dll
GAME_OBJDIR     = obj
GAME_SRCDIR     = Source/GameName
GAME_TARGET     = Program.exe

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
all: game content libs

clean:
	rm -rf $(GAME_CONTENTFILE) $(BINDIR) $(GAME_OBJDIR)

libs:
	mkdir -p $(BINDIR)
	-cp -nr $(MONOGAME_PATH)/* $(BINDIR)

run:
	cd $(BINDIR); \
	mono $(GAME_TARGET)

#-------------------
# MONO
#-------------------

# Always recompile. Makes it easier to work on the project.
.PHONY: $(BINDIR)/$(ENGINE_TARGET) engine
.PHONY: $(BINDIR)/$(GAME_TARGET) game

$(BINDIR)/$(ENGINE_TARGET):
	mkdir -p $(BINDIR)
	$(ENGINE_COMPILER) $(ENGINE_FLAGS)                        \
	            $(addprefix -lib:, $(ENGINE_LIBPATHS)) \
	            $(addprefix -r:, $(ENGINE_LIBS))       \
	            -out:$(BINDIR)/$(ENGINE_TARGET)        \
	            -recurse:$(ENGINE_SRCDIR)/*.cs

$(BINDIR)/$(GAME_TARGET): engine
	mkdir -p $(BINDIR)
	$(GAME_COMPILER) $(GAME_FLAGS)                        \
	            $(addprefix -lib:, $(GAME_LIBPATHS)) \
	            $(addprefix -r:, $(GAME_LIBS))       \
	            -out:$(BINDIR)/$(GAME_TARGET)        \
	            -recurse:$(GAME_SRCDIR)/*.cs

engine: $(BINDIR)/$(ENGINE_TARGET)


game: $(BINDIR)/$(GAME_TARGET)

#-------------------
# MONOGAME
#-------------------

# Find all content to build with MonoGame Content Builder.
CONTENT := $(shell find $(GAME_CONTENTDIR) -type f)

# Kind of a hack to build content easily.
.PHONY: $(GAME_CONTENTDIR)/*/* pre-content content

$(GAME_CONTENTDIR)/Models/*.fbx:
	@echo /build:$@ >> $(CONTENTFILE)

$(GAME_CONTENTDIR)/Textures/*.png:
	@echo /build:$@ >> $(CONTENTFILE)

pre-content:
	@echo /compress                        > $(CONTENTFILE)
	@echo /intermediateDir:$(GAME_OBJDIR) >> $(CONTENTFILE)
	@echo /outputDir:$(BINDIR)            >> $(CONTENTFILE)
	@echo /quiet                          >> $(CONTENTFILE)

content: pre-content $(CONTENT)
	mkdir -p $(BINDIR)
	mgcb -@:$(CONTENTFILE)
	rm -f $(CONTENTFILE)
