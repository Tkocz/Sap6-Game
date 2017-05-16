#!/usr/bin/make -f

# NOTE: This Makefile requires Mono and MonoGame. And does not support shitty
#       operating systems (e.g. Windows). :-)

#---------------------------------------
# CONSTANTS
#---------------------------------------

# Common config
C_BINDIR = bin
C_FLAGS  = -debug+ -define:DBG_MENU -define:DEBUG -define:MT_PHYS -define:TRACE

# Engine config
E_COMPILER   = mcs
E_FLAGS      = $(C_FLAGS) -target:library
E_LIBPATHS   = $(MONOGAME_PATH)
E_LIBS       = MonoGame.Framework.dll Lidgren.Network.dll
E_SRCDIR     = Source/EngineName
E_TARGET     = EngineName.dll

# Game config
G_COMPILER    = mcs
G_CONTENTDIR  = Content
G_CONTENTFILE = Content.mgcb
G_FLAGS       = $(C_FLAGS) -target:winexe
G_LIBPATHS    = $(C_BINDIR) $(MONOGAME_PATH)
G_LIBS        = EngineName.dll MonoGame.Framework.dll
G_OBJDIR      = obj
G_SRCDIR      = Source/GameName
G_TARGET      = Program.exe
G_TMPDIR      = tmp

#---------------------------------------
# INITIALIZATION
#---------------------------------------

# Find all content to build with MonoGame Content Builder.
CONTENT = $(shell find $(G_CONTENTDIR) -type f)

# Linux and macOS have different paths to the MonoGame library files, so make
# sure to set them up properly. No Windows support here, lol!
OS := $(shell uname)

ifeq "$(OS)" "Linux"
MGCB_PLATFORM = Linux
MONOGAME_PATH = /usr/lib/mono/xbuild/MonoGame/v3.0
endif

ifeq "$(OS)" "Darwin"
MGCB_PLATFORM = MacOSX
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
	rm -fr $(C_BINDIR) $(G_CONTENTFILE) $(G_OBJDIR) $(G_TMPDIR) doc

doc:
	doxygen

engine: $(C_BINDIR)/$(E_TARGET)

game: $(C_BINDIR)/$(G_TARGET)

libs:
	mkdir -p $(C_BINDIR)
	-cp -nr $(MONOGAME_PATH)/* $(C_BINDIR)

run:
	cd $(C_BINDIR); \
	mono $(G_TARGET)

#-------------------
# ASSEMBLIES
#-------------------

# Always recompile. Makes it easier to work on the project.
.PHONY: $(C_BINDIR)/$(E_TARGET) engine
.PHONY: $(C_BINDIR)/$(G_TARGET) game

$(C_BINDIR)/$(E_TARGET):
	mkdir -p $(C_BINDIR)
	$(E_COMPILER) $(E_FLAGS)                        \
	              $(addprefix -lib:, $(E_LIBPATHS)) \
	              $(addprefix -r:, $(E_LIBS))       \
	              -out:$(C_BINDIR)/$(E_TARGET)      \
	              -recurse:$(E_SRCDIR)/*.cs

$(C_BINDIR)/$(G_TARGET): engine
	mkdir -p $(C_BINDIR)
	$(G_COMPILER) $(G_FLAGS)                        \
	              $(addprefix -lib:, $(G_LIBPATHS)) \
	              $(addprefix -r:, $(G_LIBS))       \
	              -out:$(C_BINDIR)/$(G_TARGET)      \
	              -recurse:$(G_SRCDIR)/*.cs

#-------------------
# GAME CONTENT
#-------------------

# Kind of a hack to build content easily.
.PHONY: $(G_CONTENTDIR)/*/* $(G_CONTENTDIR)/*/*/* pre-content content

$(G_CONTENTDIR)/Fonts/*.spritefont:
	@echo /importer:FontDescriptionImporter   >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /processor:FontDescriptionProcessor >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /build:$@                           >> $(G_TMPDIR)/$(G_CONTENTFILE)

$(G_CONTENTDIR)/Models/*.blend:
	@echo /importer:OpenAssetImporter               >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /processor:ModelProcessor           >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /processorParam:TextureFormat=Color >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /build:$@                           >> $(G_TMPDIR)/$(G_CONTENTFILE)

$(G_CONTENTDIR)/Models/*.fbx:
	@echo /importer:FbxImporter               >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /processor:ModelProcessor           >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /processorParam:TextureFormat=Color >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /build:$@                           >> $(G_TMPDIR)/$(G_CONTENTFILE)

$(G_CONTENTDIR)/Models/*.x:
	@echo /importer:OpenAssetImporter         >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /processor:ModelProcessor           >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /processorParam:TextureFormat=Color >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /build:$@                           >> $(G_TMPDIR)/$(G_CONTENTFILE)

$(G_CONTENTDIR)/Sounds/Effects/*.mp3:
	@echo /importer:Mp3Importer           >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /processor:SoundEffectProcessor >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /build:$@                       >> $(G_TMPDIR)/$(G_CONTENTFILE)

$(G_CONTENTDIR)/Sounds/Effects/*.wav:
	@echo /importer:WavImporter           >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /processor:SoundEffectProcessor >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /build:$@                        >> $(G_TMPDIR)/$(G_CONTENTFILE)

$(G_CONTENTDIR)/Sounds/Music/*.mp3:
	@echo /importer:Mp3Importer    >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /processor:SongProcessor >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /build:$@                >> $(G_TMPDIR)/$(G_CONTENTFILE)

$(G_CONTENTDIR)/Textures/*.jpg:
	@echo /importer:TextureImporter   >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /processor:TextureProcessor >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /build:$@                   >> $(G_TMPDIR)/$(G_CONTENTFILE)

$(G_CONTENTDIR)/Textures/*.png:
	@echo /importer:TextureImporter   >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /processor:TextureProcessor >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /build:$@                   >> $(G_TMPDIR)/$(G_CONTENTFILE)

pre-content:
	mkdir -p $(G_TMPDIR)
	@echo /compress                     > $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /intermediateDir:$(G_OBJDIR) >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /outputDir:$(C_BINDIR)       >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /platform:$(MGCB_PLATFORM)   >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /profile:HiDef               >> $(G_TMPDIR)/$(G_CONTENTFILE)
	@echo /quiet                       >> $(G_TMPDIR)/$(G_CONTENTFILE)

content: pre-content $(CONTENT)
	mkdir -p $(C_BINDIR)
	mgcb -@:$(G_TMPDIR)/$(G_CONTENTFILE)
