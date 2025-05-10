import os

def is_text_file(file_path):
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            f.read()
        return True
    except:
        return False

def collect_all_contents(base_dir, output_file):
    numFiles = 0
    with open(output_file, 'w', encoding='utf-8') as out:
        for root, dirs, files in os.walk(base_dir):
            for file in files:
                file_path = os.path.join(root, file)
                if file_path == output_file:
                    continue  # Don't include the output file itself
                if not file_path.endswith(".cs"):
                    continue
                numFiles += 1
                print(file_path)
                out.write(f"\n\n--- FILE: {os.path.relpath(file_path, base_dir)} ---\n")
                if is_text_file(file_path):
                    try:
                        with open(file_path, 'r', encoding='utf-8') as f:
                            out.write(f.read())
                    except Exception as e:
                        out.write(f"\n[Error reading file: {e}]\n")
                else:
                    out.write("[Binary file skipped]\n")
    print(f"Total files collected : {numFiles}")

if __name__ == "__main__":
    current_dir = os.path.dirname(os.path.abspath(__file__))
    output_path = os.path.join(current_dir, 'all_contents.txt')
    collect_all_contents(current_dir, output_path)
    print(f"All readable contents saved to {output_path}")
