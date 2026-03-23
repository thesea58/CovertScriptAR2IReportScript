const fs = require('fs');
const path = require('path');

const inputFile = path.join(__dirname, 'rpx.js');
const outputDir = path.join(__dirname, 'rpx_folder');

// Create output directory if it doesn't exist
if (!fs.existsSync(outputDir)) {
  fs.mkdirSync(outputDir, { recursive: true });
}

const data = JSON.parse(fs.readFileSync(inputFile, 'utf8'));

for (const [filename, entry] of Object.entries(data)) {
  if (entry.base64) {
    const buffer = Buffer.from(entry.base64, 'base64');
    const outputPath = path.join(outputDir, filename);
    fs.writeFileSync(outputPath, buffer);
    console.log(`Decoded: ${filename} -> rpx_folder/${filename}`);
  } else {
    console.warn(`Skipped: ${filename} (no base64 field)`);
  }
}

console.log('Done.');
